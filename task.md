Bạn là Senior Full-stack Engineer. Hãy đọc kỹ toàn bộ codebase hiện tại của dự án UGem trước khi đưa ra bất kỳ plan hoặc code nào.

Nhiệm vụ của bạn là phân tích và lên kế hoạch implement phần monetization cho hệ thống, bao gồm:

1. Affiliate Tracking
2. Reviewer Commission
3. Platform Fee
4. Revenue Split
5. Reviewer Balance / Commission History
6. Payment success / failed / cancelled / refunded handling
7. Các cập nhật liên quan đến Order, Payment, Restaurant, Reviewer, Affiliate, Merchant

QUAN TRỌNG:

- Chưa code ngay.
- Chưa tạo migration ngay.
- Chưa tự ý tạo bảng mới.
- Trước tiên phải inspect codebase, database schema, entity/model, migration, service, controller hiện tại.
- Sau khi inspect xong, hãy đưa ra implementation plan chi tiết.
- Chỉ bắt đầu code sau khi tôi approve plan.

==================================================
I. PROJECT CONTEXT
==================================================

UGem là nền tảng kết nối người dùng với các quán ăn underrated.

Các actor chính:

- Customer: người dùng tìm quán, đặt món/thanh toán.
- Reviewer: người đánh giá, có thể tạo affiliate link và nhận hoa hồng.
- Merchant: chủ quán, nhận doanh thu sau khi trừ phí nền tảng và hoa hồng reviewer.
- Admin/Staff: quản lý, kiểm duyệt, cấu hình hệ thống.

Monetization flow cần làm:

Reviewer tạo affiliate link cho một restaurant.
Customer click vào affiliate link.
System tracking lượt click/session.
Customer tạo order/payment.
Nếu payment thành công, system tính:

- Hoa hồng cho Reviewer.
- Phí nền tảng cho Platform.
- Số tiền còn lại Merchant nhận.

==================================================
II. SCOPE CẦN IMPLEMENT
==================================================

Cần làm:

1. Affiliate link generation

- Reviewer tạo link affiliate cho restaurant.
- Link cần có tracking token/code unique.
- Link dùng để tracking Customer đến từ Reviewer nào.

2. Affiliate click/session tracking

- Khi Customer click link, system ghi nhận tracking session.
- Session có thời hạn.
- Dùng session để attribute order sau này.

3. Order attribution

- Khi Customer tạo order, system kiểm tra có affiliate session hợp lệ không.
- Nếu có, order được gắn với affiliate session/reviewer.
- Không tính commission khi order mới được tạo.
- Commission chỉ tính khi payment thành công.

4. Reviewer commission calculation

- Tính hoa hồng Reviewer dựa trên rank và bonus nếu có.
- Snapshot rate tại thời điểm payment success.
- Không tính lại commission cũ khi rank thay đổi sau này.

5. Platform fee

- Tính phí nền tảng.
- Phase đầu có thể dùng config cố định.
- Nếu codebase đã có UnderratedScore hoặc PlatformFee logic thì reuse.

6. Revenue split

- Tính số tiền chia cho từng bên:
  - ReviewerFee
  - PlatformFee
  - MerchantReceive

7. Reviewer balance / earning history

- Cập nhật số dư Reviewer.
- Lưu lịch sử commission/earning.
- Có khả năng reverse khi refund.

8. Payment status handling

- Pending
- Paid / PaymentSuccess
- Failed
- Cancelled
- Refunded

9. Backend API

- Tạo hoặc cập nhật API liên quan đến affiliate, commission, balance, order/payment.

10. Frontend update nếu project có frontend

- Reviewer xem/tạo affiliate link.
- Reviewer xem earning/balance.
- Customer click affiliate link được redirect đúng.
- Không thêm UI voucher.

11. Test

- Unit test cho business logic.
- Integration test nếu codebase có test infrastructure.

==================================================
III. OUT OF SCOPE — KHÔNG LÀM
==================================================

Tạm thời KHÔNG implement Voucher.

Không làm:

- Không tạo voucher table.
- Không tạo voucher validation.
- Không tạo voucher claim flow.
- Không tạo voucher wallet.
- Không tạo apply voucher button.
- Không tạo discount code.
- Không implement voucher lifecycle.
- Không dùng voucher trong công thức tính tiền.

Vì phase này không làm voucher, công thức tiền sẽ dùng OrderTotalAmount hoặc FinalAmount hiện tại của order.

Nếu codebase đã có discount khác không phải voucher:

- Không xóa.
- Không phá logic cũ.
- Chỉ không thêm voucher mới.

Không làm payment gateway thật nếu chưa có:

- Nếu codebase chưa có payment provider, chỉ tạo abstraction hoặc mock endpoint đủ để test flow commission.
- Không build full banking/wallet/payout integration.

Không làm payout thật:

- Không chuyển tiền thật ra ngân hàng.
- Chỉ lưu balance và transaction history.
- Payout thật để phase sau.

==================================================
IV. MONEY FORMULA / BUSINESS RULES
==================================================

Vì không làm voucher trong phase này:

FinalAmount = OrderTotalAmount

Nếu codebase đã có field FinalAmount sẵn thì reuse.
Nếu codebase chỉ có TotalAmount thì dùng TotalAmount như FinalAmount.

Công thức chuẩn:

ReviewerFee = FinalAmount × ReviewerCommissionRate

PlatformFee = FinalAmount × PlatformFeeRate

MerchantReceive = FinalAmount - ReviewerFee - PlatformFee

Yêu cầu bắt buộc:

- Không dùng float/double để tính tiền.
- Dùng decimal hoặc money-safe type theo convention codebase.
- Nếu hệ thống dùng VND integer thì follow convention hiện tại.
- Nếu hệ thống dùng decimal thì nên dùng decimal(18,2) hoặc type tương đương.
- Commission và fee phải được snapshot tại thời điểm payment success.
- Không tính lại commission cũ nếu rank/platform fee thay đổi sau này.
- Mỗi order chỉ được tạo tối đa 1 commission record.
- Payment success event/webhook có thể bị gọi nhiều lần, logic phải idempotent.
- Tất cả update liên quan đến order, commission, balance, revenue split phải nằm trong transaction nếu stack hỗ trợ.

==================================================
V. REVIEWER RANK / COMMISSION RATE
==================================================

Default rank và commission rate:

Bronze:

- Point: 0–19
- Commission: 0%

Silver:

- Point: 20–49
- Commission: 0.5%

Gold:

- Point: 50–99
- Commission: 1%

Diamond:

- Point: >= 100
- Commission: 2%

Nếu codebase đã có ReviewerRank:

- Reuse enum/model hiện tại.
- Không tạo enum mới nếu không cần.
- Không duplicate rank logic.

Nếu codebase chưa có ReviewerRank:

- Chưa tự ý tạo bảng mới.
- Có thể đề xuất thêm field vào bảng Reviewer/User hiện tại, ví dụ:
  - ReviewerPoints
  - ReviewerRank
- Nhưng phải báo rõ trong plan trước.

Rank calculation:

- Nếu hệ thống đã lưu rank trực tiếp thì dùng rank đó.
- Nếu hệ thống lưu point thì tạo service resolve rank từ point.
- Không implement full mission/anti-abuse system trong phase này nếu chưa có sẵn.
- Chỉ cần rank đủ để tính commission.

==================================================
VI. PROPOSAL BONUS
==================================================

Rule:
Nếu Reviewer là người đã đề xuất restaurant và restaurant đó đã được approve, Reviewer được cộng bonus commission.

Default:
ProposalBonusRate = 1%

EffectiveCommissionRate = RankCommissionRate + ProposalBonusRate

Chỉ áp dụng bonus nếu:

- Reviewer là proposer của đúng restaurant đó.
- Proposal đã approved.
- Restaurant đang active.
- Reviewer hiện vẫn hợp lệ.

Không áp dụng bonus nếu:

- Proposal pending.
- Proposal rejected.
- Reviewer không phải proposer của restaurant đó.
- Reviewer chỉ tạo affiliate link nhưng không phải người đề xuất quán.

Nếu codebase đã có RestaurantProposal/OnboardingRequest:

- Reuse bảng/entity đó.

Nếu codebase chưa có proposal tracking:

- Check xem Restaurant hiện tại có field nào như ProposedByUserId, CreatedBy, SubmittedBy, ReviewerId không.
- Ưu tiên reuse/extend field hiện tại.
- Nếu thật sự cần tạo bảng mới để tracking proposal thì phải hỏi tôi trước.

==================================================
VII. AFFILIATE LINK RULES
==================================================

Reviewer có thể tạo affiliate link cho từng restaurant.

Affiliate link cần map được các thông tin:

- AffiliateLinkId hoặc AffiliateId hiện tại
- ReviewerId
- RestaurantId hoặc MerchantId
- TrackingCode / TrackingToken
- CreatedAt
- IsActive
- Optional: ExpiredAt
- Optional: ClickCount

QUAN TRỌNG VỀ DATABASE:
Các field trên là concept nghiệp vụ, không có nghĩa là phải tạo bảng AffiliateLinks mới.
Phải inspect database hiện tại trước.

Yêu cầu:

- TrackingCode phải unique, random, không đoán được.
- Không để client truyền ReviewerId/MerchantId trực tiếp để fake tracking.
- Public link nên dạng:
  - /a/{trackingCode}
    hoặc route phù hợp với frontend hiện tại.
- Reviewer chỉ được tạo link nếu có role Reviewer.
- Không cho tạo link cho restaurant inactive/rejected/deleted.
- Nếu link đã tồn tại cho cùng Reviewer + Restaurant, có thể return link cũ thay vì tạo duplicate, tùy convention codebase.
- Có API để Reviewer xem các link đã tạo.
- Có thể hiển thị số click/conversion nếu dữ liệu hiện tại hỗ trợ.

Edge cases:

1. User không phải Reviewer cố tạo affiliate link.
2. Reviewer tạo link cho restaurant không tồn tại.
3. Reviewer tạo link cho restaurant inactive.
4. Reviewer tạo duplicate link cho cùng restaurant.
5. TrackingCode bị trùng.
6. TrackingCode sai hoặc inactive.

==================================================
VIII. AFFILIATE CLICK / SESSION TRACKING
==================================================

Khi Customer click affiliate link:

Main flow:

1. Customer mở public affiliate link.
2. System resolve trackingCode.
3. System kiểm tra affiliate link active không.
4. System kiểm tra restaurant active không.
5. System tạo affiliate tracking session/click.
6. System lưu session/click.
7. System redirect Customer đến restaurant detail page.

Affiliate session/click cần map được:

- AffiliateSessionId hoặc AffiliateClickId nếu có
- AffiliateLinkId hoặc AffiliateId
- ReviewerId
- RestaurantId
- CustomerId nullable
- AnonymousSessionId nullable
- TrackingToken
- ClickedAt
- ExpiresAt
- ConvertedOrderId nullable
- Status: Active / Converted / Expired / Invalid
- Optional: IpHash
- Optional: UserAgentHash
- Optional: Referrer

QUAN TRỌNG:
Các field trên là concept. Không được tự tạo bảng AffiliateSessions/AffiliateClicks mới nếu chưa hỏi tôi.
Nếu bảng Affiliate hiện tại đang gộp session/click data thì hãy phân tích trước.

Attribution window:

- Default: 7 ngày.
- Có thể đưa vào config.
- Nếu codebase đã có config system thì reuse.

Attribution rule:

- Chỉ attribute order nếu order cùng RestaurantId với affiliate session.
- Chỉ attribute nếu session còn hạn.
- Nếu Customer click nhiều affiliate link cho cùng một restaurant, dùng last-click attribution.
- Nếu Customer click nhiều link cho nhiều restaurant khác nhau, chỉ session matching với restaurant của order được dùng.
- Nếu Customer tự là Reviewer của link đó, không tính commission self-referral.
- Nếu Customer là Merchant/Owner của restaurant đó, không tính commission.
- Nếu link inactive tại thời điểm conversion, default là không tính commission.
- Nếu Reviewer bị banned/deactivated tại thời điểm payment success, không tính commission hoặc đưa vào PendingReview nếu codebase có trạng thái đó.

Không lưu raw IP nếu không cần.
Nếu cần anti-abuse, chỉ lưu hash.

==================================================
IX. ORDER ATTRIBUTION
==================================================

Khi Customer tạo order:

1. System kiểm tra request/session/cookie/local storage xem có affiliate tracking không.
2. Resolve affiliate session hợp lệ.
3. Kiểm tra session còn hạn.
4. Kiểm tra restaurant trong order trùng với restaurant của affiliate session.
5. Kiểm tra không self-referral.
6. Nếu hợp lệ, gắn affiliate session/link vào order hoặc attribution record.
7. Không tạo commission ở bước này.

Commission chỉ tạo khi payment success.

Nếu order không thanh toán:

- Không tạo commission.
- Không update reviewer balance.
- Không final revenue split.

Nếu codebase hiện tại không có Order module:

- Đề xuất minimal order model/abstraction.
- Không build full order/payment system nếu ngoài scope.
- Có thể tạo mock payment success endpoint để test flow.

==================================================
X. PAYMENT STATUS HANDLING
==================================================

Cần xử lý các trạng thái:

1. Pending

- Order mới tạo hoặc đang chờ thanh toán.
- Không tính commission.
- Không update balance.

2. Paid / PaymentSuccess

- Tạo commission nếu order có affiliate attribution hợp lệ.
- Tạo revenue split snapshot.
- Cập nhật reviewer balance.
- Mark affiliate session converted.
- Ghi lịch sử transaction/ledger nếu có.
- Tất cả chạy trong transaction.
- Idempotent: gọi lại nhiều lần không tạo duplicate.

3. Failed

- Không tính commission.
- Không update balance.
- Không mark converted.

4. Cancelled

- Không tính commission.
- Không update balance.
- Nếu đã có hold/session thì giữ ở trạng thái không converted.

5. Refunded

- Nếu order đã tạo commission, phải reverse commission.
- Reverse platform fee/revenue split nếu có ghi nhận.
- Trừ lại reviewer balance nếu commission đã available.
- Nếu balance không đủ, tạo negative adjustment hoặc debt record tùy convention codebase.
- Không xóa record cũ, nên lưu trạng thái Reversed để audit.

Idempotency:

- Payment success có thể bị gọi nhiều lần.
- Refund có thể bị gọi nhiều lần.
- Không duplicate commission.
- Không reverse nhiều lần.
- Cần unique constraint hoặc logic check theo OrderId.

==================================================
XI. COMMISSION LIFECYCLE
==================================================

Commission status đề xuất:

- Pending
- Available / Approved
- Reversed
- Cancelled

Default đơn giản:

- Khi payment success, tạo commission status = Available.
- Nếu codebase muốn hold refund window thì có thể tạo Pending trước.
- Nếu chưa có background job infrastructure, không bắt buộc làm pending-to-available job.

Commission record cần map được:

- Id
- OrderId
- ReviewerId
- RestaurantId/MerchantId
- AffiliateLinkId hoặc AffiliateId
- AffiliateSessionId nếu có
- FinalAmount
- RankAtPurchase
- RankCommissionRate
- ProposalBonusRate
- EffectiveCommissionRate
- CommissionAmount
- Status
- CreatedAt
- AvailableAt nullable
- ReversedAt nullable
- Reason nullable

QUAN TRỌNG:
Không tự tạo bảng ReviewerCommissions nếu database hiện tại đã có bảng Affiliate/Reviewer/Transaction đang gộp logic này.
Phải inspect trước.

Reviewer balance cần map được:

- ReviewerId
- TotalEarned
- AvailableBalance
- PendingBalance nếu có
- TotalReversed hoặc ReversedAmount
- UpdatedAt

Không chỉ lưu mỗi balance mà không có lịch sử.
Cần có transaction/ledger/history để audit.
Nếu codebase đã có transaction/history table thì reuse.

==================================================
XII. PLATFORM FEE
==================================================

Platform fee phase đầu có thể dùng config cố định.

Default config đề xuất:

- BaseFeeRate = 5%
- GrowthFactor = 0% nếu chưa có dynamic fee

Nếu codebase đã có UnderratedScore / US:
Có thể dùng công thức:

PlatformFeeRate = BaseFeeRate + GrowthFactor × (1 - UnderratedScore)

Trong đó:

- UnderratedScore = 1 nghĩa là quán vắng, platform fee thấp.
- UnderratedScore = 0 nghĩa là quán đông, platform fee cao.

Nếu codebase chưa có UnderratedScore:

- Tạo service IPlatformFeeService hoặc PlatformFeeService.
- Implementation hiện tại có thể trả về BaseFeeRate cố định.
- Thiết kế để sau này thay bằng dynamic fee mà không sửa order/payment code.

Platform fee phải được snapshot tại thời điểm payment success.
Không tính lại order cũ khi config thay đổi.

Không tự tạo bảng PlatformFeeConfigs nếu codebase có config system hiện tại.
Nếu muốn tạo bảng config mới, phải hỏi tôi trước.

==================================================
XIII. REVENUE SPLIT
==================================================

Khi payment success, cần tạo hoặc lưu snapshot revenue split.

Revenue split cần map được:

- OrderId
- GrossAmount / FinalAmount
- ReviewerCommissionAmount
- PlatformFeeAmount
- MerchantReceivableAmount
- ReviewerCommissionRate
- PlatformFeeRate
- CreatedAt

Formula:

MerchantReceivableAmount = FinalAmount - ReviewerCommissionAmount - PlatformFeeAmount

Validation:

- MerchantReceivableAmount không được âm.
- Nếu EffectiveCommissionRate + PlatformFeeRate vượt ngưỡng cho phép, phải reject hoặc cap theo config.
- Default max total fee rate có thể là 30%, đưa vào config nếu codebase hỗ trợ.
- Rounding phải nhất quán.
- Nếu có lệch do rounding, đưa phần lệch vào PlatformFee hoặc follow convention hiện tại.

Không tự tạo bảng OrderFinancials/RevenueSplits nếu Order/Payment hiện tại có thể lưu snapshot.
Nếu muốn tạo bảng riêng để audit, phải hỏi tôi trước.

==================================================
XIV. DATABASE / MIGRATION RULE — CỰC KỲ QUAN TRỌNG
==================================================

Hiện tại database có thể đang gộp các nghiệp vụ affiliate, reviewer, commission, balance hoặc order financial vào một số bảng sẵn có như:

- Affiliate
- Reviewer
- Order
- Payment
- Restaurant
- User
- Transaction
- Wallet
- Earning
- v.v.

Vì vậy, KHÔNG được tự ý tạo các bảng mới như:

- AffiliateLinks
- AffiliateSessions
- AffiliateClicks
- ReviewerCommissions
- ReviewerBalances
- ReviewerBalanceTransactions
- CommissionLedger
- OrderFinancials
- RevenueSplits
- PlatformFeeConfigs
- RestaurantProposal

Các tên bảng trên chỉ là concept nghiệp vụ để tham khảo, không phải yêu cầu bắt buộc tạo bảng.

Trước tiên bắt buộc phải inspect:

- Database schema hiện tại
- Entity/model hiện tại
- Migration hiện tại
- Relationship hiện tại
- Existing service/repository
- Existing controller/API
- Existing DTO/ViewModel

Nhiệm vụ bắt buộc:

1. Kiểm tra hiện tại có những bảng/entity nào đang liên quan đến:

- Affiliate
- Reviewer
- Commission
- Balance
- Order
- Payment
- Restaurant proposal / proposer
- Platform fee
- Revenue split

2. Xác định logic hiện tại đang được gộp ở bảng nào.

Ví dụ:

- Nếu bảng Affiliate hiện tại đã chứa ReviewerId, RestaurantId, TrackingCode thì ưu tiên mở rộng bảng này thay vì tạo AffiliateLinks mới.
- Nếu bảng Reviewer đã có point/rank/balance thì ưu tiên reuse hoặc thêm field vào Reviewer thay vì tạo ReviewerBalances mới.
- Nếu Order/Payment đã có amount/status/fee thì ưu tiên thêm snapshot field vào bảng hiện tại thay vì tạo OrderFinancials mới.
- Nếu đã có transaction/history table thì reuse thay vì tạo CommissionLedger mới.

3. Chỉ được đề xuất tạo bảng mới nếu:

- Bảng hiện tại không thể mở rộng hợp lý.
- Việc gộp tiếp sẽ gây sai normalization hoặc khó audit.
- Cần tách riêng để đảm bảo idempotency.
- Cần transaction log/history riêng.
- Cần lưu nhiều dòng lịch sử commission.
- Bảng hiện tại không chứa đủ relationship cần thiết.

4. Nếu muốn tạo bất kỳ bảng mới nào, PHẢI hỏi ý kiến tôi trước.

Không được tự tạo migration thêm bảng mới khi chưa được approve.

5. Khi đề xuất thay đổi database, phải chia thành 2 nhóm:

A. Reuse / extend existing tables

- Bảng nào reuse được?
- Thêm field nào?
- Có cần index/constraint không?
- Có ảnh hưởng dữ liệu cũ không?

B. Proposed new tables, cần tôi approve trước

- Bảng nào muốn tạo?
- Vì sao cần tạo?
- Vì sao không reuse bảng cũ?
- Risk nếu không tạo?
- Alternative là gì?

6. Với mỗi bảng mới được đề xuất, phải giải thích:

- Vì sao bảng hiện tại không đủ dùng.
- Vì sao không nên gộp vào bảng cũ.
- Bảng mới giải quyết vấn đề gì.
- Field chính.
- Quan hệ với bảng cũ.
- Risk nếu không tạo bảng mới.
- Có alternative nào không.

7. Nếu có thể implement bằng cách thêm field vào bảng hiện tại, hãy ưu tiên phương án đó trong phase đầu.

8. Không được phá dữ liệu cũ.
   Không được rename/drop column/table nếu chưa có lý do rõ ràng và chưa được tôi approve.

9. Nếu cần migration:

- Migration phải backward-compatible nhất có thể.
- Không làm mất dữ liệu.
- Có default value hợp lý.
- Có nullable strategy nếu dữ liệu cũ chưa đủ.
- Có backfill plan nếu cần.
- Không drop table/column cũ khi chưa approve.

10. Trong plan, bắt buộc xuất ra bảng mapping:

Existing Table | Current Purpose | Can Reuse? | Needed Changes | Need New Table? | Reason

Ví dụ format:

Affiliate | Đang gộp affiliate data | Yes | Add TrackingCode, IsActive, CreatedAt | No | Có thể mở rộng
Reviewer | Đang lưu reviewer info | Yes | Add Rank/Points nếu thiếu | No | Không cần ReviewerBalances phase đầu
Order | Đang lưu order/payment amount | Yes | Add AffiliateId, PlatformFee, ReviewerFee, MerchantReceive | No | Có thể snapshot trực tiếp

Database decision rule:

- Default: reuse existing tables.
- Create new table only after explicit approval from me.

IMPORTANT:
Do not create any new database table without asking me first.
The current database already groups some of these concepts into existing tables.
Your first task is to inspect and propose whether to reuse/extend existing tables.
Any new table must be explicitly approved by me before implementation.

==================================================
XV. API ENDPOINTS — ĐỀ XUẤT, KHÔNG BẮT BUỘC Y NGUYÊN
==================================================

Hãy kiểm tra route style hiện tại trước.
Không tự tạo route khác convention nếu codebase đã có pattern.

Reviewer endpoints đề xuất:

POST /api/restaurants/{restaurantId}/affiliate-links

- Reviewer tạo affiliate link cho restaurant.

GET /api/reviewer/affiliate-links

- Reviewer xem danh sách affiliate links.

GET /api/reviewer/commissions

- Reviewer xem commission history.

GET /api/reviewer/balance

- Reviewer xem balance/earning summary.

Public/customer endpoint đề xuất:

GET /a/{trackingCode}

- Customer mở affiliate link.
- System tracking click/session.
- Redirect sang restaurant detail.

Hoặc nếu frontend/backend tách riêng:

GET /api/affiliate/{trackingCode}/track

Order/payment integration:

Nếu đã có order/payment endpoints:

- Update logic trong create order và payment success handler.

Nếu chưa có:
POST /api/orders
POST /api/orders/{orderId}/mark-paid
POST /api/orders/{orderId}/cancel
POST /api/orders/{orderId}/refund

Admin/Staff optional:

GET /api/admin/commissions
GET /api/admin/revenue-splits
GET /api/admin/platform-fee-config
PUT /api/admin/platform-fee-config

Không nhất thiết phải implement toàn bộ admin endpoints nếu codebase chưa có admin area.
Nhưng phải ghi rõ trong plan phần nào làm, phần nào để phase sau.

==================================================
XVI. FRONTEND REQUIREMENTS
==================================================

Nếu codebase có frontend, hãy inspect trước.

Nếu frontend đã có role Reviewer:
Cần thêm hoặc update:

- Màn hình Affiliate Links
- Reviewer chọn restaurant và tạo link
- Nút copy affiliate link
- Danh sách link đã tạo
- Click count/conversion count nếu backend hỗ trợ
- Màn hình Earnings:
  - Available balance
  - Total earned
  - Commission history
  - Commission status

Nếu frontend có restaurant detail:

- Public affiliate link redirect đến restaurant detail.
- Preserve tracking token/session để order sau đó attribute được.

Nếu frontend có checkout/order:

- Không thêm voucher field.
- Không thêm apply voucher button.
- Khi tạo order, gửi tracking/session info nếu backend cần.
- Sau payment success, hiển thị order success bình thường.

Nếu frontend chưa đủ:

- Ưu tiên backend trước.
- Ghi rõ frontend tasks trong plan.
- Không build UI lớn ngoài scope nếu chưa cần.

==================================================
XVII. SECURITY / ANTI-ABUSE
==================================================

Cần xử lý:

1. Không cho Reviewer tự mua qua link của chính mình để ăn commission.
2. Không cho Merchant/Owner tự mua qua affiliate link của quán mình.
3. Không tính commission cho order failed/cancelled/refunded.
4. Không duplicate commission.
5. Không tin ReviewerId/MerchantId từ client.
6. TrackingCode phải random và không đoán được.
7. Rate limit tạo affiliate link nếu project có rate limiting.
8. Audit log hoặc structured logging cho:
   - affiliate link created
   - affiliate click tracked
   - order attributed
   - payment success
   - commission created
   - commission reversed
9. Không lưu thông tin cá nhân nhạy cảm nếu không cần.
10. Nếu lưu IP/UserAgent để anti-abuse, ưu tiên hash.

==================================================
XVIII. EDGE CASES BẮT BUỘC PHẢI COVER
==================================================

Affiliate link:

1. Reviewer tạo link cho restaurant không tồn tại.
2. Reviewer tạo link cho restaurant inactive/rejected/deleted.
3. User thường không phải Reviewer cố tạo link.
4. Reviewer tạo duplicate link cho cùng restaurant.
5. TrackingCode sai.
6. TrackingCode expired.
7. TrackingCode inactive.

Affiliate tracking: 8. Customer click link nhưng không order. 9. Customer click link A rồi link B cùng quán, dùng last-click. 10. Customer click link quán A nhưng order quán B, không tính commission. 11. Customer click link đã inactive, không tính commission.

Order/payment: 12. Customer order nhưng payment pending, không tính commission. 13. Customer order nhưng payment failed, không tính commission. 14. Customer cancel order, không tính commission. 15. Payment success event gọi 2 lần, không duplicate commission. 16. Order refunded, commission reversed. 17. Refund event gọi 2 lần, không reverse duplicate. 18. Concurrent payment success requests.

Reviewer/merchant abuse: 19. Reviewer tự mua qua link của mình. 20. Merchant tự mua qua affiliate link của quán mình. 21. Reviewer bị deactivated/banned trước payment success. 22. Reviewer rank thay đổi sau order, commission cũ không đổi. 23. Platform fee config thay đổi sau order, order cũ không đổi.

Proposal bonus: 24. Reviewer là proposer approved của restaurant, có +1%. 25. Reviewer không phải proposer, không có bonus. 26. Proposal pending/rejected, không có bonus. 27. Restaurant tự đăng ký, không có reviewer proposal bonus.

Money: 28. Total fee vượt ngưỡng cho phép. 29. MerchantReceive bị âm. 30. Rounding tiền lẻ. 31. Order amount bằng 0 hoặc âm. 32. Commission rate null/missing. 33. Platform fee config missing.

Database: 34. Existing table đã có field tương tự, không tạo field duplicate. 35. Migration không làm mất dữ liệu cũ. 36. Không tạo bảng mới khi chưa được approve.

==================================================
XIX. TESTING REQUIREMENTS
==================================================

Viết test cho business logic trước nếu project có test infrastructure.

Unit tests cần có:

1. Reviewer rank to commission rate

- Bronze = 0%
- Silver = 0.5%
- Gold = 1%
- Diamond = 2%

2. Proposal bonus

- Approved proposer được +1%
- Non-proposer không được bonus
- Pending/rejected proposal không được bonus

3. Effective commission rate

- Rank rate + proposal bonus
- Không vượt max fee nếu có config

4. Platform fee calculation

- Fixed base fee
- Dynamic fee nếu codebase có UnderratedScore

5. Revenue split calculation

- ReviewerFee đúng
- PlatformFee đúng
- MerchantReceive đúng
- MerchantReceive không âm
- Rounding đúng

6. No voucher path

- Không có voucher
- FinalAmount = OrderTotalAmount hoặc field hiện tại

7. Payment status

- Pending không tạo commission
- Paid tạo commission
- Failed không tạo commission
- Cancelled không tạo commission
- Refunded reverse commission

Integration tests nếu có thể:

1. Reviewer creates affiliate link.
2. Customer clicks affiliate link.
3. Customer places order.
4. Payment success creates commission.
5. Payment failed does not create commission.
6. Duplicate payment success does not duplicate commission.
7. Refund reverses commission.
8. Last-click attribution.
9. Expired session no commission.
10. Self-referral no commission.
11. Merchant self-purchase no commission.

Nếu project chưa có test infrastructure:

- Đề xuất test plan cụ thể.
- Implement ít nhất unit tests cho service core nếu khả thi.

==================================================
XX. CODING STYLE / ARCHITECTURE
==================================================

Follow architecture hiện tại của codebase.

Không được:

- Không viết business logic trực tiếp trong controller.
- Không hard-code rate ở nhiều nơi.
- Không duplicate model/enum đã có.
- Không phá route convention.
- Không đổi tên bảng/field cũ nếu không cần.
- Không drop dữ liệu cũ.
- Không tạo bảng mới khi chưa hỏi.

Nên có service rõ ràng, tùy codebase:

- AffiliateLinkService
- AffiliateTrackingService
- CommissionService
- PlatformFeeService
- RevenueSplitService
- ReviewerBalanceService
- OrderPaymentHandler hoặc PaymentService update

Nếu codebase đã có service tương tự:

- Reuse và mở rộng service hiện tại.
- Không tạo service mới trùng trách nhiệm.

Config:

- Commission rate theo rank nên nằm trong config/service mapping.
- Platform fee rate nên nằm trong config/service.
- Attribution window nên nằm trong config.
- Max total fee rate nên nằm trong config.

Logging:

- Log payment success handling.
- Log commission creation.
- Log commission reversal.
- Log skipped commission reason.

Transaction:

- Payment success handler phải chạy transaction.
- Commission creation + balance update + revenue split update phải atomic nếu stack hỗ trợ.

==================================================
XXI. OUTPUT MONG MUỐN TRƯỚC KHI CODE
==================================================

Sau khi đọc codebase, hãy trả lời theo format sau.
Chưa code trước khi trả lời format này.

1. Current codebase analysis

- Stack đang dùng:
  - Backend framework
  - Frontend framework nếu có
  - ORM/database
  - Auth/role system
  - Test framework

- Existing modules liên quan:
  - User
  - Role
  - Reviewer
  - Merchant
  - Restaurant
  - Affiliate
  - Order
  - Payment
  - Transaction/Wallet/Balance
  - Admin/Staff

- Những model/entity hiện tại có thể reuse.
- Những service/controller hiện tại có thể reuse.
- Những phần đang thiếu.

2. Database reuse analysis

Bắt buộc trả lời:

- Hiện tại những bảng nào đang gộp affiliate/reviewer/commission/order/payment?
- Có thể reuse bảng nào?
- Cần thêm field nào vào bảng cũ?
- Có bảng mới nào bạn muốn tạo không?
- Nếu có, phải hỏi tôi approve trước.
- Không được tạo migration thêm bảng mới khi chưa được tôi đồng ý.

Bắt buộc có bảng mapping:

Existing Table | Current Purpose | Can Reuse? | Needed Changes | Need New Table? | Reason

3. Proposed implementation plan

Chia theo phase:

Phase 1: Database/schema analysis

- Reuse bảng nào?
- Extend bảng nào?
- Có cần bảng mới không?
- Migration plan nếu được approve.

Phase 2: Backend services

- Service nào cần tạo/sửa.
- Business logic chính.

Phase 3: API endpoints

- Endpoint nào thêm/sửa.
- Permission/role.
- Request/response DTO.

Phase 4: Order/payment integration

- Gắn affiliate vào order như thế nào.
- Payment success xử lý như thế nào.
- Refund xử lý như thế nào.

Phase 5: Frontend updates

- Màn hình nào thêm/sửa.
- Không thêm voucher UI.

Phase 6: Tests

- Unit tests.
- Integration tests.
- Manual test cases.

Phase 7: Config/seeding

- Commission rates.
- Platform fee config.
- Attribution window.
- Max total fee rate.

4. Data model changes

Với mỗi bảng/entity cần sửa:

- Tên bảng/entity.
- Field thêm.
- Type.
- Nullable hay required.
- Default value.
- Index/unique constraint.
- Relationship.
- Migration risk.

Nếu muốn tạo bảng mới:

- Dừng lại và hỏi tôi approve.
- Không tự code migration.

5. API changes

Với mỗi endpoint:

- Method + route.
- Mục đích.
- Role được phép gọi.
- Request body.
- Response body.
- Error cases.
- Service xử lý.

6. Business rules confirmation

Xác nhận rõ:

- Không làm voucher.
- FinalAmount lấy từ OrderTotalAmount/current final amount.
- Commission chỉ tạo khi payment success.
- Payment failed/cancelled không tạo commission.
- Refund reverse commission.
- Attribution dùng last-click.
- Session expiry default 7 ngày.
- Self-referral không tính commission.
- Merchant self-purchase không tính commission.
- Proposal bonus +1% nếu reviewer là approved proposer.
- Platform fee dùng config hoặc service hiện tại.
- Snapshot rate tại payment success.

7. Risk / assumptions

Ghi rõ:

- Codebase đang thiếu gì.
- Assumption nào bạn đang đưa ra.
- Phần nào cần tôi xác nhận.
- Phần nào có thể gây breaking change.
- Phần nào nên để phase sau.

8. Implementation order

Liệt kê cụ thể:

- File nào sẽ đọc.
- File nào dự kiến sửa.
- File nào dự kiến thêm.
- Thứ tự task nhỏ.
- Cách test sau mỗi task.
- Điểm nào cần tôi approve trước.

==================================================
XXII. APPROVAL RULE
==================================================

Bạn phải chờ tôi approve trong các trường hợp sau:

1. Muốn tạo bảng mới.
2. Muốn drop/rename column/table.
3. Muốn thay đổi relationship quan trọng.
4. Muốn thay đổi auth/role logic.
5. Muốn thay đổi payment/order status enum hiện tại.
6. Muốn thay đổi API contract cũ.
7. Muốn thay đổi cách tính tiền hiện tại nếu codebase đã có logic cũ.
8. Muốn thêm background job.
9. Muốn implement payout thật.
10. Muốn thêm voucher hoặc discount logic mới.

Không cần hỏi nếu:

- Chỉ đọc code.
- Chỉ phân tích.
- Chỉ đề xuất plan.
- Chỉ viết test plan.
- Chỉ reuse/extend service không phá logic cũ.

==================================================
XXIII. FINAL REMINDER
==================================================

Tóm lại:

- Hãy đọc codebase trước.
- Không code ngay.
- Không làm voucher.
- Không tự tạo bảng mới.
- Ưu tiên reuse bảng/entity/service hiện tại.
- Nếu muốn tạo bảng mới, phải hỏi tôi approve trước.
- Cần đưa plan chi tiết theo format yêu cầu.
- Sau khi tôi approve plan mới bắt đầu implement.

Mục tiêu là thêm monetization vào hệ thống hiện tại một cách an toàn:
Affiliate → Tracking → Order Attribution → Payment Success → Commission → Platform Fee → Revenue Split → Reviewer Balance.
