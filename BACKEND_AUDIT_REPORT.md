# BACKEND AUDIT REPORT

## 1. Executive Summary
- Overall quality score: `7.4/10`
- Production readiness: `Moderate`, improved meaningfully by this pass but not yet fully hardened.
- Biggest risks:
  - Order lifecycle still mixes payment completion and fulfillment confirmation in one `Status` field.
  - The QR/check-in path remains coupled to order completion and can still create data-integrity risk, but that module was intentionally left frozen.
  - `MailKit 4.15.1` still reports a known moderate vulnerability at build time.
- Scalability concerns:
  - Merchant discovery still depends on aggregate subqueries per merchant row.
  - Review and moderation flows are serviceable now, but the domain still relies on string status values rather than a stricter state model.
  - Background rebalancing was improved substantially, but long-term growth will still benefit from pre-aggregated metrics or dedicated reporting tables.

## 2. Critical Issues

### Issue: Committed secrets in source-controlled configuration
- Severity: `Critical`
- Affected files:
  - `UGem.Api/appsettings.json`
  - `UGem.Api/appsettings.Development.json`
  - `UGem.Api/Program.cs`
- Root cause: database, JWT, Cloudinary, and mail credentials were stored directly in appsettings files.
- Real-world impact: credential leakage could allow full database access, token forgery, media-account abuse, and SMTP abuse.
- Exploit scenario: anyone with repo or deployment artifact access could extract secrets and impersonate the backend or third-party integrations.
- Recommended fix: moved secrets to environment-driven configuration with startup validation and placeholder detection.
- Status in this pass: `Mitigated`

### Issue: Payment webhook accepted unauthenticated requests
- Severity: `Critical`
- Affected files:
  - `UGem.Api/Controllers/OrderController.cs`
  - `UGem.Services/OrderService/Service.cs`
  - `UGem.Api/Program.cs`
- Root cause: `/api/v1/orders/sepay/webhook` is still a public integration endpoint and SePay does not support the custom verification header model introduced in the previous hardening pass.
- Real-world impact: an attacker could mark orders paid or failed by posting crafted webhook bodies.
- Exploit scenario: a public POST with a guessed order reference and matching amount could transition order state.
- Recommended fix: keep defensive payload validation, idempotency, and suspicious-attempt logging now; adopt provider-supported signing or IP allowlisting later if SePay exposes it.
- Status in this pass: `Partially mitigated`

### Issue: Customer order-detail endpoint exposed IDOR risk
- Severity: `Critical`
- Affected files:
  - `UGem.Api/Controllers/OrderController.cs`
  - `UGem.Services/OrderService/Service.cs`
- Root cause: `GET /api/v1/orders/{id}` read order details by order ID without scoping the query to the authenticated customer.
- Real-world impact: authenticated customers could enumerate other customers' order line items.
- Exploit scenario: a user could swap route IDs and read another customer's purchase details.
- Recommended fix: scope the detail query to the caller's `CustomerId` claim.
- Status in this pass: `Mitigated`

### Issue: QR/check-in integrity risk remains in completion coupling
- Severity: `Critical`
- Affected files:
  - `UGem.Services/OrderService/Service.cs`
  - QR/check-in module is frozen and was not modified
- Root cause: order completion still interacts with check-in creation, while payment completion and delivery confirmation share the same status model.
- Real-world impact: duplicate or mistimed completion flows can still create inconsistent check-in side effects.
- Exploit scenario: repeated completion behavior around the frozen module can inflate visit/check-in state.
- Recommended fix: report only for now due freeze; revisit after the QR/check-in module is unfrozen.
- Status in this pass: `Documented only`

## 3. Medium Issues

### Issue: Reviewer and application status values are still inconsistent across flows
- Severity: `Medium`
- Affected files:
  - `UGem.Services/Application/Service.cs`
  - `UGem.Services/StaffService/Service.cs`
- Root cause: business status values use free-form strings such as `Pending`, `Approved`, `Accept`, and `Rejected`.
- Real-world impact: inconsistent UI/state handling and harder reporting logic.
- Recommended fix: normalize status values behind enums or centralized constants in a later pass.

### Issue: MailKit dependency vulnerability remains unresolved
- Severity: `Medium`
- Affected files:
  - `UGem.Services/UGem.Services.csproj`
- Root cause: `MailKit 4.15.1` is flagged by `NU1902`.
- Real-world impact: known vulnerable dependency remains in the production dependency graph.
- Recommended fix: upgrade MailKit to a patched version after compatibility verification.

### Issue: Exception taxonomy is still uneven across services
- Severity: `Medium`
- Affected files:
  - Multiple service classes
- Root cause: several flows still throw generic `Exception` rather than domain-appropriate exception types.
- Real-world impact: less precise HTTP status mapping and harder telemetry analysis.
- Recommended fix: continue replacing generic exceptions with `KeyNotFoundException`, `InvalidOperationException`, and `UnauthorizedAccessException`.

## 4. Minor Issues

### Issue: Unused constructor dependency remains in application service
- Severity: `Minor`
- Affected files:
  - `UGem.Services/Application/Service.cs`
- Root cause: media service is injected but not used.
- Real-world impact: mild maintainability noise.
- Recommended fix: remove if not needed after confirming no near-term upload handling is planned there.

### Issue: Merchant discovery still computes review aggregates inline
- Severity: `Minor`
- Affected files:
  - `UGem.Services/MerchantService/Service.cs`
- Root cause: rating and review counts are projected from related reviews per query.
- Real-world impact: acceptable at current scale, but may become expensive under higher traffic.
- Recommended fix: revisit with precomputed aggregates if merchant volume grows materially.

## 5. Performance Bottlenecks
- `UGem.Services/BackGroundJobService/RebalancingJob.cs`
  - The original per-merchant repeated aggregate queries were a major bottleneck.
  - This pass replaced them with batched grouped lookups keyed by merchant ID.
- `UGem.Services/MerchantService/Service.cs`
  - Read paths now use `AsNoTracking()` and tighter pagination, but merchant search/map still depends on live aggregate projection.
- `UGem.Services/OrderService/Service.cs`
  - Order creation now batches order and order details into one save, reducing write chatter.
- `UGem.Services/Application/Service.cs`
  - Application creation and acceptance now avoid unnecessary intermediate saves and redundant includes.
- `UGem.Services/ReviewService/Service.cs`
  - Review detail validation was converted from repeated per-item lookups to batched reads.

## 6. Security Risks
- Secret sprawl in config was the highest-risk issue and has been mitigated with placeholders plus startup validation.
- Public media upload previously lacked authentication and robust validation; this pass added authentication, size limits, MIME checks, and safer failure handling.
- Webhook shared-secret validation was removed because the current SePay integration model does not support custom verification headers. Retained defenses are strict order-reference parsing, amount validation, duplicate-completion prevention, safe state checks, and warning logs for suspicious payloads.
- The QR/check-in module remains a sensitive area with integrity risk, but it was intentionally left untouched.
- Build output still reports the MailKit advisory and should be treated as an open security item.

## 7. Recommended Refactor Priority
1. Upgrade `MailKit` and re-run regression checks.
2. Normalize order state semantics so payment confirmation and fulfillment confirmation are not encoded in the same field.
3. Normalize application/reviewer status values behind shared constants or enums.
4. Continue replacing generic exceptions in the remaining services for cleaner API semantics.
5. Revisit the frozen QR/check-in integration once that module is allowed to change.
