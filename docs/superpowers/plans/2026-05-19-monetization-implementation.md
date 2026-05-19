# Monetization System Implementation Plan (Revised)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a centralized monetization system for UGem, covering affiliate tracking, reviewer commissions, platform fees, and revenue splitting.

**Architecture:** A centralized `MonetizationService` handles all financial logic. `Order` entity stores snapshots of fees and an idempotency marker. `ReviewerWalletTransaction` serves as an audit ledger and secondary idempotency guard.

**Tech Stack:** ASP.NET Core, EF Core (PostgreSQL), Guid PKs.

---

### Task 1: Database Schema & Entity Update

**Files:**
- Create: `UGem.Repositories/Entity/ReviewerWalletTransaction.cs`
- Modify: `UGem.Repositories/Entity/Order.cs`
- Modify: `UGem.Repositories/AppDbContext.cs`

- [ ] **Step 1: Create ReviewerWalletTransaction Entity**
  Define ledger table with:
  - `Guid Id` (PK)
  - `Guid ReviewerId` (FK to Reviewer)
  - `Guid OrderId` (FK to Order)
  - `decimal Amount`
  - `string Type` (Use string constants "Commission", "Reversal")
  - `DateTimeOffset CreatedAtUtc`
  - `decimal BalanceAfter`
  - `string? Reason`

- [ ] **Step 2: Update Order Entity**
  - Add `public DateTimeOffset? MonetizationProcessedAtUtc { get; set; }` to `UGem.Repositories/Entity/Order.cs`.
  - Confirm precision for `ReviewerFee`, `PlatformFee` is already `decimal(18,2)` in `AppDbContext`.

- [ ] **Step 3: Update AppDbContext Configuration**
  - Register `DbSet<ReviewerWalletTransaction>`.
  - Configure `ReviewerWalletTransaction` precision: `Amount` (18,2), `BalanceAfter` (18,2).
  - Configure **Unique Constraint** on `ReviewerWalletTransaction`: `OrderId + Type`.
  - Set up relationships (Order has many WalletTransactions, Reviewer has many WalletTransactions).

- [ ] **Step 4: Create Migration**
  - Run: `dotnet ef migrations add AddMonetizationLedger`
  - **Review Step**: Show migration diff. **DO NOT APPLY** until approved.

---

### Task 2: Monetization Service - Core Logic

**Files:**
- Create: `UGem.Services/MonetizationService/IService.cs`
- Create: `UGem.Services/MonetizationService/Service.cs`
- Create: `UGem.Services/MonetizationService/Models.cs`
- Modify: `UGem.Api/Program.cs`

- [ ] **Step 1: Register Service in DI**
  - In `UGem.Api/Program.cs`, add `builder.Services.AddScoped<MonetizationService.IService, MonetizationService.Service>();`.

- [ ] **Step 2: Implement HandlePaymentSuccess (Idempotent)**
  - Guard: Check `Order.MonetizationProcessedAtUtc != null`.
  - Validation: Ensure `Order.OrderDetails` all belong to the **same Merchant**.
  - Validation: `FinalPrice > 0`, `PlatformFeePercent >= 0`.
  - Calculate `PlatformFee` for every order.
  - If `AffiliateLinkId` exists:
    - Validate `AffiliateLink.IsActive`.
    - Check Self-Referral: `Order.Customer.UserId == Reviewer.Customer.UserId`.
    - Check Merchant-Self-Purchase: `Order.Customer.UserId == Merchant.UserId`.
    - Resolve Rank Commission Rate.
    - Calculate `ReviewerFee`.
    - If `ReviewerFee >= 0`, create `ReviewerWalletTransaction` (Type="Commission") and update `Reviewer.Balance`.
  - Snapshot: Update `Order.PlatformFee`, `Order.ReviewerFee`, `Order.MonetizationProcessedAtUtc`.

- [ ] **Step 3: Implement HandleRefund (Idempotent)**
  - Guard: Check if `Commission` record exists and `Reversal` does not.
  - Reverse original `ReviewerFee` snapshot from `Order` if > 0.
  - Update `Reviewer.Balance`.
  - Create `ReviewerWalletTransaction` (Type="Reversal").

- [ ] **Step 4: Propose Commit**
  - Suggest message: `feat(service): implement centralized monetization service with ledger audit`

---

### Task 3: Order Flow Integration

**Files:**
- Modify: `UGem.Services/OrderService/Service.cs`
- Modify: `UGem.Services/OrderService/Request.cs` (DTO)

- [ ] **Step 1: Update CreateOrder DTO**
  - Add optional `string? LinkCode` to the creation request.

- [ ] **Step 2: Update Order Creation Logic**
  - If `LinkCode` provided, resolve `AffiliateLink` from DB.
  - Validate `AffiliateLink.MerchantId` matches Order's Merchant.
  - Set `Order.AffiliateLinkId`.

- [ ] **Step 3: Trigger Monetization in MarkAsPaid**
  - Inject `IMonetizationService`.
  - Call `HandlePaymentSuccess(orderId)` after `PaymentStatus` is updated to "Paid".
  - **Ownership**: `OrderService` methods should wrap these in a single transaction if they use `SaveChangesAsync()`.

- [ ] **Step 4: Propose Commit**
  - Suggest message: `feat(integration): link affiliate on order creation and trigger monetization on payment`

---

### Task 4: Affiliate Click Tracking & Redirects

**Files:**
- Modify: `UGem.Services/AffiliateLinkService/Service.cs`

- [ ] **Step 1: Update Click Tracking**
  - Increment `ClickCount`.
  - Document client-side TTL: Frontend stores `linkCode` + `expiresAt` (7 days). Backend ignores client timestamp.

---

### Task 5: Testing & Validation

- [ ] **Step 1: Unit Test Monetization Formulas**
  - Bronze (0%), Silver (0.5%), Gold (1%), Diamond (2%).
  - Revenue Split: `MerchantReceive = FinalPrice - ReviewerFee - PlatformFee`.
  - Invalid inputs: Negative price/fees.

- [ ] **Step 2: Integration Test Idempotency**
  - Multiple `HandlePaymentSuccess` calls -> One Ledger record.
  - Multiple `HandleRefund` calls -> One Reversal record.

- [ ] **Step 3: Test Edge Cases**
  - Non-affiliate paid order: `PlatformFee` snapshotted, `ProcessedAtUtc` set, No ledger entry.
  - Multiple Merchants in one order: Ensure failure/rejection.
  - Self-referral: Commission = 0, but ledger entry created (0-value).
  - Bronze Reviewer: Commission = 0, ledger entry created.

- [ ] **Step 4: Manual Validation**
  - Review Migration Diff.
  - Verify Database state after simulated payment.
