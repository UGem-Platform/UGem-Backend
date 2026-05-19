# Monetization System Design Spec (Revised)

**Date:** 2026-05-19  
**Status:** Final Draft (Awaiting Implementation)  
**Goal:** Implement a centralized monetization system for UGem, covering affiliate tracking, reviewer commissions, platform fees, and revenue splitting.

---

## 1. Architecture Overview
- **MonetizationService**: A centralized backend service for all financial calculations.
- **Ledger-Based Audit**: `ReviewerWalletTransaction` table tracks all balance changes and ensures idempotency for commissions.
- **General Idempotency**: `Order.MonetizationProcessedAtUtc` field tracks when monetization logic has been executed for an order (both affiliate and non-affiliate).
- **MVP Attribution**: Client-side tracking (LinkCode + ExpiresAt) with backend validation.

---

## 2. Data Model Changes

### 2.1 Existing Tables (Reuse & Extend)
- **Order**: 
  - **Snapshot Fields**: `ReviewerFee`, `PlatformFee` (Decimal 18,2).
  - **New Field (APPROVED)**: `DateTimeOffset? MonetizationProcessedAtUtc`. 
    - *Purpose*: The primary idempotency marker for all monetization processing.
- **Reviewer**: Update `Balance` (Decimal 18,2).
- **Merchant**: Use `PlatformFeePercent` (Decimal 5,2). *Note: 5.00 represents 5%.*
- **AffiliateLink**: Used for order attribution (`AffiliateLinkId`).

### 2.2 New Table: `ReviewerWalletTransaction` (APPROVED)
- `Id` (Guid, PK)
- `ReviewerId` (Guid, FK to `Reviewer`, Indexed)
- `OrderId` (Guid, FK to `Order`, Indexed)
- `Amount` (Decimal 18,2) — Always stored as positive.
- `Type` (String/Enum: `Commission`, `Reversal`)
- `CreatedAtUtc` (DateTimeOffset)
- `BalanceAfter` (Decimal 18,2) — Snapshot after transaction.
- `Reason` (String, Nullable)
- **Unique Constraint**: `OrderId + Type`.

---

## 3. Business Rules & Formulas

### 3.1 Reviewer Rank & Rates
- **Bronze** (0-19 pts): 0%
- **Silver** (20-49 pts): 0.5%
- **Gold** (50-99 pts): 1.0%
- **Diamond** (100+ pts): 2.0%

### 3.2 Formulas
- `EffectiveCommissionRate = RankCommissionRate`
- `ReviewerFee = FinalPrice * EffectiveCommissionRate`
- `PlatformFee = FinalPrice * (Merchant.PlatformFeePercent / 100)`
- `MerchantReceive (Dynamic) = FinalPrice - ReviewerFee - PlatformFee`

---

## 4. MonetizationService Workflow

### 4.1 HandlePaymentSuccess(orderId)
1. **Idempotency**: Check `Order.MonetizationProcessedAtUtc`. If not null, **Exit**.
2. **Context**: Load `Order` (include `OrderDetails.Food`, `Customer`, `AffiliateLink.Reviewer.Customer`, `AffiliateLink.Merchant`).
3. **Identify Merchant**: Get `MerchantId` from the first `OrderDetail.Food.MerchantId`.
4. **Validation (Affiliate Logic)**:
   - If `Order.AffiliateLinkId` is null -> `ReviewerFee = 0`.
   - If `Order.AffiliateLinkId` exists:
     - Validate `AffiliateLink.IsActive == true` and `AffiliateLink.MerchantId == OrderMerchantId`.
     - **Self-Referral Guard**: Compare `Order.Customer.UserId` with `AffiliateLink.Reviewer.Customer.UserId`. If match, `ReviewerFee = 0`.
     - **Merchant-Purchase Guard**: Compare `Order.Customer.UserId` with `Merchant.UserId`. If match, `ReviewerFee = 0`.
5. **Calculate**:
   - Calculate `PlatformFee` for the merchant (snapshot into `Order`).
   - Calculate `ReviewerFee` based on Rank if affiliate exists.
6. **Snapshot**: `Order.ReviewerFee`, `Order.PlatformFee`, and set `Order.MonetizationProcessedAtUtc = Now`.
7. **Apply Money (Reviewer)**:
   - If `Order.AffiliateLinkId` is not null:
     - Update `Reviewer.Balance` (even if fee is 0).
     - Create `ReviewerWalletTransaction` (Type=Commission, Amount=ReviewerFee, BalanceAfter=Reviewer.Balance).
8. **Commit**: All operations wrapped in a DB transaction.

### 4.2 HandleRefund(orderId)
1. **Validation**: 
   - Check if `ReviewerWalletTransaction(OrderId=orderId, Type=Commission)` exists.
   - Check if `ReviewerWalletTransaction(OrderId=orderId, Type=Reversal)` exists (Exit if yes).
2. **Calculate**: Retrieve original `ReviewerFee` from `Order` snapshot or ledger.
3. **Apply**:
   - Decrement `Reviewer.Balance` if original fee > 0.
   - Create `ReviewerWalletTransaction` (Type=Reversal, Amount=OriginalReviewerFee).
4. **Commit**: Save changes in DB transaction.

---

## 5. MVP Attribution Flow
- **Client Storage**: `linkCode` (string) and `expiresAt` (timestamp) in `localStorage`.
- **Backend Validation**:
  - Backend ignores `expiresAt` from client and uses its own logic (resolves `AffiliateLink` from DB, checks `IsActive`).
  - Validation ensures `AffiliateLink.MerchantId` matches the order's primary `MerchantId`.

---

## 6. Implementation Constraints
- **Idempotency**: `Order.MonetizationProcessedAtUtc` is the master flag for the process; `OrderId + Type` is the guard for the ledger.
- **Safety**: No `float/double`. Non-nullable fee fields in `Order` mean 0 is a value; the `ProcessedAt` timestamp is the only way to know if logic ran.
- **Limitation**: Client-side attribution is subject to clearing storage; backend only trusts the `linkCode` existence and validity.
