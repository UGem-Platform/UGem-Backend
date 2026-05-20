# UGem.Api v1 Frontend API Contract

This file stores the normalized frontend API contract for `UGem.Api v1`.

## Base URL

```
https://ugem-test-backend.onrender.com
```

## Auth

- `Authorization` header is required for protected endpoints.
- Header format:

```
Authorization: Bearer <token>
Content-Type: application/json
```

### Response envelope

```ts
{
  success: boolean;
  message: string;
  data?: any;
  errors?: any;
  traceId?: string;
  timestampUtc: string;
}
```

### Standard error body

```ts
{
  success: false;
  message: string;
  errors?: {
    code?: string;
    details?: any;
  };
  traceId?: string;
  timestampUtc: string;
}
```

---

## 1. AUTH

### Login

```http
POST /api/v1/auth/login
```

#### Request

```ts
LoginRequest {
  email: string;
  password: string;
}
```

#### Response data

```ts
IdentityResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}
```

### Refresh token

```http
POST /api/v1/auth/refresh-token
```

#### Request

```ts
RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}
```

#### Notes

- Does not require `Authorization` header.
- The access token may be expired, but the refresh token must match the stored active DB record.
- Refresh rotates tokens: store the returned `refreshToken` and discard the old one.

#### Response data

```ts
IdentityResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}
```

### Register

```http
POST /api/v1/auth/register
```

#### Request

```ts
RegisterUserRequest {
  email: string;
  password: string;
  phoneNumber: string;
  fullName: string;
  role: string;
}
```

---

## 2. CUSTOMERS

### Get customer profile

```http
GET /api/v1/customers/profile
```

### Search customers by phone number

```http
GET /api/v1/customers/search-by-phone-number?phoneNumber={phoneNumber}&limit={limit}
```

### Search customers by email

```http
GET /api/v1/customers/search-by-email?email={email}&limit={limit}
```

#### Notes

- Auth: `Merchant`
- `limit` is clamped from `1` to `20`.

#### Response item shape

```ts
SearchCustomerResponse {
  userId: string;
  customerId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: "Customer" | "Reviewer";
  avatarUrl?: string;
}
```

---

## 3. MERCHANTS

### Search merchants

```http
GET /api/v1/merchants
```

#### Query parameters

```ts
{
  searchTerm?: string;
  pageIndex?: number;
  pageSize?: number;
}
```

#### Response item shape

```ts
MerchantSummaryResponse {
  id: string;
  name: string;
  description: string;
  address: string;
  logoUrl: string;
  rating: number;
  reviewCount: number;
  restaurantType?: string;
  mainDishType?: string;
  priceRange?: string;
  distance?: number;
  latitude?: number;
  longitude?: number;
}
```

### Get merchant by id

```http
GET /api/v1/merchants/{id}
```

#### Response shape

```ts
MerchantDetailResponse {
  id: string;
  name: string;
  description: string;
  address: string;
  logoUrl: string;
  rating: number;
  reviewCount: number;
  restaurantType?: string;
  mainDishType?: string;
  priceRange?: string;
  email: string;
  phone: string;
  latitude: number;
  longitude: number;
  menu: Food[];
}
```

### Get merchants by category

```http
GET /api/v1/merchants/by-category
```

#### Query parameters

```ts
{
  categoryId: string;
  pageIndex?: number;
  pageSize?: number;
}
```

### Get merchants for map

```http
GET /api/v1/merchants/map
```

#### Query parameters

```ts
{
  minLongitude: number;
  maxLongitude: number;
  minLatitude: number;
  maxLatitude: number;
  zoomLevel: number;
}
```

#### Response item shape

```ts
MerchantMapResponse {
  id: string;
  name: string;
  description: string;
  address: string;
  logoUrl: string;
  rating: number;
  reviewCount: number;
  restaurantType?: string;
  mainDishType?: string;
  priceRange?: string;
  latitude: number;
  longitude: number;
}
```
## Update Merchant
```http
PUT /api/v1/merchants
```
### Request Body

{
"merchantName": "string",
"merchantDescription": "string",
"restaurantType": "string",
"mainDishType": "string",
"priceRange": "string",
"email": "string",
"phone": "string",
"address": "string",
"openingHours": "string"
}
### Response
{
"success": true,
"message": "Merchant updated successfully",
"data": null
}
---

## 4. CATEGORIES

### Create category

```http
POST /api/v1/categories
```

### Get all categories

```http
GET /api/v1/categories
```

### Get child categories

```http
GET /api/v1/categories/{parentId}/children
```
### create new categories

```http
Post /api/v1/categories
```
Auth: Staff
#### Request:
{
"parentId": "can be null",
"name": "string",
"description": "string"
}
#### Response
{
"success": true,
"message": "Category added successfully",
"data": "1",
"errors": null,
"traceId": "string",
"timestampUtc": "2026-05-06T07:40:00Z"
}

---

## 5. FOODS

### Create food

```http
POST /api/v1/foods
```
Request:
{
"name": "string",
"description": "string",
"price": number
}

Response:
{
"success": true,
"message": "Add food Successfully"
}

### Get foods

```http
GET /api/v1/foods
```

### Get food by id

```http
GET /api/v1/foods/{id}
```
### Delete foods

```http
DELETE /api/v1/foods/{id}
```
#### Request: foodId
#### Response :
{
"success": true,
"message": "Delete Food Successfully",
"data": null,
"errors": null,
"traceId": "string",
"timestampUtc": "2026-05-06T07:56:00.8414088Z"
}

---
# Food Topping APIs

---

## Create Food Topping

Description:
Merchant thêm topping cho món ăn.

Authorization:
Bearer Token (Merchant)

Method:
POST

Request URL:

```txt
/api/v1/food-toppings
```

Request Body:

```json
{
  "foodId": "09c2daa1-5f3e-45e7-9fc2-79938264c296",
  "name": "Them Trung",
  "price": 10000
}
```

Response:

```json
{
  "success": true,
  "message": "Create food topping successfully",
  "data": null,
  "errors": null,
  "traceId": "0HNLC9OGCSF9I:00000004"
}
```

---

## Get Food Toppings

Description:
Lấy danh sách topping của món ăn.

Authorization:
No Authorization

Method:
GET

Request URL:

```txt
/api/v1/foods/{foodId}/toppings
```

Example:

```txt
/api/v1/foods/09c2daa1-5f3e-45e7-9fc2-79938264c296/toppings
```

Response:

```json
{
  "success": true,
  "message": "Get food toppings successfully",
  "data": [
    {
      "id": "30000000-0000-0000-0000-000000000001",
      "foodId": "09c2daa1-5f3e-45e7-9fc2-79938264c296",
      "name": "Them Trung",
      "price": 10000,
      "isActive": true
    },
    {
      "id": "30000000-0000-0000-0000-000000000002",
      "foodId": "09c2daa1-5f3e-45e7-9fc2-79938264c296",
      "name": "Them Cha",
      "price": 15000,
      "isActive": true
    }
  ],
  "errors": null,
  "traceId": "0HNLC9OGCSF9I:00000004"
}
```

---

## Update Food Topping

Description:
Merchant cập nhật topping.

Authorization:
Bearer Token (Merchant)

Method:
PUT

Request URL:

```txt
/api/v1/food-toppings
```

Request Body:

```json
{
  "foodToppingId": "30000000-0000-0000-0000-000000000001",
  "name": "Them Trung Ga"
}
```

Or:

```json
{
  "foodToppingId": "30000000-0000-0000-0000-000000000001",
  "price": 12000
}
```

Or:

```json
{
  "foodToppingId": "30000000-0000-0000-0000-000000000001",
  "isActive": false
}
```

Description:
Only fields sent in request body will be updated.
```

Response:

```json
{
  "success": true,
  "message": "Update food topping successfully",
  "data": null,
  "errors": null,
  "traceId": "0HNLC9OGCSF9I:00000004"
}
```

---

## Delete Food Topping

Description:
Merchant xóa topping.

Authorization:
Bearer Token (Merchant)

Method:
DELETE

Request URL:

```txt
/api/v1/food-toppings/{foodToppingId}
```

Example:

```txt
/api/v1/food-toppings/30000000-0000-0000-0000-000000000001
```

Response:

```json
{
  "success": true,
  "message": "Delete food topping successfully",
  "data": null,
  "errors": null,
  "traceId": "0HNLC9OGCSF9I:00000004"
}
```

---

## 6. ORDERS

### Create order (customer)

```http
POST /api/v1/orders
```

#### Request

```ts
CreateOrderRequest {
  name: string;
  paymentMethod: string;
  notes: string;
  deliveryAddress: string;
  foods: FoodOrderRequest[];
}
```

#### FoodOrderRequest

```ts
{
  foodId: string;
  quantity: number;
}
```

#### Response shape

```ts
CreateOrderResponse {
  orderId: string;
  totalAmount: number;
  bankName: string;
  bankAccount: string;
  description: string;
  code: string;
  qrCode: string;
}
```

### SePay webhook

```http
POST /api/v1/orders/sepay/webhook
```

#### Notes

- This route is used by the SePay integration.
- No custom verification header is currently required because the active SePay integration model does not support it.
- The backend still rejects malformed order references, invalid amounts, unknown orders, duplicate completion, and invalid order states.

### Get merchant orders

```http
GET /api/v1/orders
```

### Get customer orders

```http
GET /api/v1/orders/mine
```

### Get order detail

```http
GET /api/v1/orders/{id}
```

#### Notes

- Auth: `Customer`
- The backend only returns details for orders owned by the authenticated customer.

#### Response item shape

```ts
GetOrderDetailResponse {
  name: string;
  quantity: number;
  unitPrice: number;
  notes?: string;
  foodId: string;
  orderId: string;
}
```

### Update order status

```http
PATCH /api/v1/orders/{id}/status
```

#### Request

```ts
UpdateOrderStatusRequest {
    status: "Accepted" | "Rejected" | "Completed" | "NotReceived";
    reason?: string; // required when status = Rejected
}
```

---

## 7. REVIEWS

### Get merchant reviews

```http
GET /api/v1/reviews/merchant?merchantId={merchantId}
```

#### Response item shape

```ts
MerchantReviewResponse {
  id: string;
  merchantId: string;
  orderId: string;
  rating: number;
  content: string;
  imageUrl?: string;
  createdAt: string;
  customerName?: string;
  customerAvatarUrl?: string;
}
```

### Review merchant

```http
POST /api/v1/reviews/merchant
```

#### Request

```ts
ReviewMerchantRequest {
  merchantId: string;
  orderId: string;
  rating: number;
  content: string;
  imageUrl: string;
  reviewDetails: ReviewDetailRequest[];
}
```

#### ReviewDetailRequest

```ts
{
  orderDetailId: string;
  detailContent: string;
  rating: number;
}
```

### Update review merchant

```http
PUT /api/v1/reviews/merchant
```

#### Request

```ts
UpdateReviewMerchantRequest {
  reviewId: string;
  rating: number;
  content: string;
  imageUrl: string;
  reviewDetails: UpdateReviewDetailRequest[];
}
```

#### UpdateReviewDetailRequest

```ts
{
  reviewDetailId: string;
  detailContent: string;
  rating: number;
}
```

### Get reviews by merchant

```http
GET /api/v1/reviews/merchant
```

#### Query parameters

```ts
{
  merchantId: string;
}
```

### Get review details by merchant

```http
GET /api/v1/reviews/merchant/review-details
```

#### Query parameters

```ts
{
  reviewId: string;
}
```

## 8. WISHLISTS

### Add to wishlist

```http
POST /api/v1/wishlists
```

#### Request

```ts
CreateWishlistRequest {
  merchantId: string;
}
```

### Get wishlist

```http
GET /api/v1/wishlists
```

### Remove wishlist

```http
DELETE /api/v1/wishlists/{merchantId}
```

---

## 9. APPLICATIONS

### Create application

```http
POST /api/v1/applications
```

#### Request

```ts
ApplicationRequest {
  name: string;
  description: string;
  restaurantType?: string;
  mainDishType?: string;
  priceRange?: string;
  email: string;
  phone: string;
  logoUrl: string;
  openingHours: string;
  address: string;
  latitude: number;
  longitude: number;
  menu: CreateFoodRequest[];
}
```

### Get merchant applications

```http
GET /api/v1/applications/mine
```

### Get applications (staff/admin)

```http
GET /api/v1/applications
```

### Update application

```http
PUT /api/v1/applications/{id}
```

### Update application status

```http
PATCH /api/v1/applications/{id}/status
```

#### Request

```ts
UpdateApplicationStatusRequest {
  status: "Accepted" | "Rejected";
  note?: string;
}
```

---

## 10. AFFILIATE LINKS

### Create affiliate link

```http
POST /api/v1/affiliate-links
```

### Get affiliate links

```http
GET /api/v1/affiliate-links
```

### Get affiliate link by id

```http
GET /api/v1/affiliate-links/{id}
```

---

## 11. ADMIN / STAFF

### Admin

```http
POST /api/v1/admins/staff
```
#### Request:
```
 "email": "string",
  "fullName": "string",
  "password": "string",
  "phoneNumber": "string"
  ```
#### Response:
      {
      "success": true,
      "message": "Create staff success",
      "data": null,
      "errors": null,
      "traceId": null,
      "timestampUtc": "2026-05-13T11:21:24.6080312Z"
     }
```
GET /api/v1/admins/staff
```
#### Request:
    searchTerm: string
    pageSize : int
    pageIndex: int
#### Response:
    "success": true,
    "message": "Get staff list success",
    "data": {
    "items": [
    {
    "id": "GUID",
    "userId": "GUID",
    "fullName": "string",
    "email": "string",
    "phoneNumber": "string",
    "avatarUrl": null,
    "isActive": true,
    "hiredAt": "2026-01-01T00:00:00+00:00",
    "createdAt": "2026-04-23T08:00:00+00:00"
    }
    ],
    "totalItems": 2,
    "pageSize": 10,
    "pageIndex": 1
    },
    "errors": null,
    "traceId": null,
    "timestampUtc": "2026-05-13T11:19:17.7899557Z"
    }
```

Delete/api/v1/admins/staff/{staffId}
```
#### Request:
    StaffId: Guid
#### Response:
    {
    "success": true,
    "message": "Delete staff success",
    "data": null,
    "errors": null,
    "traceId": null,
    "timestampUtc": "2026-05-13T11:25:27.0501592Z"
    }
```
GET /api/v1/admins/dashboard
```
#### Response:
    {
    "success": true,
    "message": "Get dashboard success",
    "data": {
    "totalUsers": int,
    "totalMerchants": int,
    "totalOrders": int,
    "totalRevenue": decimal,
    "newUsersToday": int,
    "pendingApplications": int,
    "pendingReviewerApplications": int
    },
    "errors": null,
    "traceId": null,
    "timestampUtc": "2026-05-13T11:26:18.7964569Z"
    }
```
### Staff

```http
```
#### Accept Reviewer Application
```
POST /api/v1/staff/accept
```
#### Request:
    id: applicationId
#### Response:
    {
      "success": true,
      "message": "Approve Successfully",
      "data": null,
      "errors": null,
      "traceId": "0HNLBB7D8SEMU:00000003",
      "timestampUtc": "2026-05-06T10:26:19.3822403Z"
    }
```
### Get All Reviewer Application
```

GET /api/v1/staff
#### Request:
    searTerm: number,
    pageSize: number,
    pageIndex: number
#### Response:
    {
      "success": true,
      "message": "GetReviewerApplications Successfully",
      "data": {
        "items": [
          {
            "id": "GUID",
            "status": "Rejected",
            "motivation": "string",
            "experience": "string",
            "facebookUrl": "string",
            "tiktokUrl": "string",
            "youtubeUrl": "string",
            "otherSocialUrl": "string",
            "rejectionReason": "string",
            "customerId": "GUID",
            "createdAt": "2026-05-06T09:49:21.500452+00:00"
          }
        ],
        "totalItems": number,
        "pageSize": number,
        "pageIndex": number
      },
      "errors": null,
      "traceId": "0HNLBB7D8SEN0:00000001",
      "timestampUtc": "2026-05-06T10:31:49.9234526Z"
    }
GET /api/v1/staff/{id}
```
#### Reject  Reviewer Application
```
POST /api/v1/staff/reject
#### Request:
    Id: applicationId,
    reason: string
#### Response:
    {
    "success": true,
    "message": "Reject Successfully",
    "data": null,
    "errors": null,
    "traceId": "0HNLBB7D8SEN1:00000001",
    "timestampUtc": "2026-05-06T10:38:09.3641842Z"
    }
```

---

## 12. NOTIFICATIONS

### Get notifications

```http
GET /api/v1/notifications
```

---

## 13. MEDIA

### Upload image

```http
POST /api/v1/media/images
```

#### Notes

- Auth: `Bearer token required`
- Content type: `multipart/form-data`
- Form field: `file`
- Max request size: `5 MB`
- Accepted image types: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`

#### Response shape

```ts
{
  url: string;
}
```

---

## 14. FRONTEND ARCHITECTURE SUGGESTION

### API layer structure

```
/api
  auth.api.ts
  customer.api.ts
  merchant.api.ts
  food.api.ts
  order.api.ts
  review.api.ts
  wishlist.api.ts
  application.api.ts
  category.api.ts
```

---
## 15. REVIEWER APPLICATIONS

### Create reviewer application

```http
POST /api/v1/reviewer-applications
```

#### Request

```ts
CreateReviewerApplicationRequest {
  motivation: string;
  experience: string;
  facebookUrl: string;
  tiktokUrl: string;
  youtubeUrl: string;
  otherSocialUrl: string;
}
```

### Update reviewer application

```http
PATCH /api/v1/reviewer-applications
```

#### Request

```ts
UpdateReviewerApplicationRequest {
  reviewerApplicationId: string;
  motivation: string;
  experience: string;
  facebookUrl: string;
  tiktokUrl: string;
  youtubeUrl: string;
  otherSocialUrl: string;
}
```

---

## 16. USERS

### Get profile

```http
GET /api/v1/user/profile
```

### Update profile

```http
PATCH /api/v1/user/profile
```

#### Request

```ts
UpdateProfileRequest {
  fullName: string;
  avatarUrl: string;
}
```

---

### Axios base client

```ts
const api = axios.create({
    baseURL: "https://ugem-test-backend.onrender.com",
    headers: {
        "Content-Type": "application/json",
    },
});
```

### Request interceptor

```ts
api.interceptors.request.use((config) => {
    const token = localStorage.getItem("token");
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
});
```
## 17.Campaign APIs:
### Get All Campaigns
```http
GET     /api/v1/campaigns
(Role customer)
```

#### Request

```ts
```
#### Response
```
{
  "success": true,
  "message": "Get campaign list successfully",
  "data": [
    {
      "id": "Guid",
      "code": "string",
      "title": "string",
      "description": "string",
      "discountValue": 50,
      "isPercentage": true,
      "minOrderAmount": 100000,
      "maxDiscountAmount": 50000,
      "quantity": int,
      "usedCount": int,
      "maxUsagePerUser": int,
      "isGlobal": bool,
      "isNewUserOnly": bool,
      "isActive": bool,
      "startDate": "DateTimeOffset",
      "endDate": "DateTimeOffset",
      "merchantId": null
    }
  ```
---
### Create capaigns(Role merchant or admin)
```http
POST    /api/v1/campaigns
```
#### Request
```
{
  "code": "string",
  "title": "string",
  "description": "string",
  "discountValue": 100000,
  "isPercentage": false,
  "minOrderAmount": decimal,
  "maxDiscountAmount": decimal,
  "quantity": int,
  "maxUsagePerUser": int,
  "isGlobal": true,
  "isNewUserOnly": false,
  "startDate": "2026-05-20T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z"
}
```
#### Response
```
{
  "success": true,
  "message": "Create campaign successfully",
  "data": "Create campaign successfully",
  "errors": null,
  "traceId": "0HNLMC2KQ88FL:00000002",
  "timestampUtc": "2026-05-20T11:30:29.9604734Z"
}
```
### Update capaigns(role merchant or admin)
```http
PUT     /api/v1/campaigns
```
#### Request
```
"code": "string",
  "title": "string",
  "description": "string",
  "discountValue": 100000,
  "isPercentage": false,
  "minOrderAmount": decimal,
  "maxDiscountAmount": decimal,
  "quantity": int,
  "maxUsagePerUser": int,
  "isGlobal": true,
  "isNewUserOnly": false,
  "startDate": "2026-05-20T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z"
  "id": "Guid"
```
#### Response
```
{
  "success": true,
  "message": "Update campaign successfully",
  "data": "Update campaign successfully",
  "errors": null,
  "traceId": "0HNLMA1NBM77L:00000002",
  "timestampUtc": "2026-05-20T09:20:26.4181968Z"
}
```

### Get capaigns By Id(role customer)
```http
GET     /api/v1/campaigns/{id}
```
#### Request
```
"id": "Guid"
```
#### Response
```
{
  "success": true,
  "message": "Get campaign successfully",
  "data": {
    "id": "440b2854-cc05-45b0-973a-b2685f5c2fc3",
    "code": "BUNBO67",
    "title": "Weekend Spicy Noodle",
    "description": "Voucher cuối tuần cho mì cay",
    "discountValue": 25,
    "isPercentage": true,
    "minOrderAmount": 90000,
    "maxDiscountAmount": 35000,
    "quantity": 150,
    "usedCount": 0,
    "maxUsagePerUser": 1,
    "isGlobal": false,
    "isNewUserOnly": false,
    "isActive": true,
    "startDate": "2026-05-20T00:00:00+00:00",
    "endDate": "2026-07-30T23:59:59+00:00",
    "merchantId": "88888888-8888-8888-8888-888888888888"
  },
  "errors": null,
  "traceId": "0HNLMA1NBM77L:00000003",
  "timestampUtc": "2026-05-20T09:20:45.684626Z"
}
```
### Delete campaign(role Merchant or Admin)
```http
DELETE  /api/v1/campaigns/{id}
```
#### Request
```
"id": "Guid"
```
#### Response
```
{
  "success": true,
  "message": "Delete campaign successfully",
  "data": "Delete campaign successfully",
  "errors": null,
  "traceId": "0HNLMA1NBM77L:00000004",
  "timestampUtc": "2026-05-20T09:20:56.8508428Z"
}
```
### Apply campaign(role customer)
```http
POST    /api/v1/campaigns/apply
```
#### Request
```
{
  "code": "WELCOME50",
  "merchantId": null
}
```
#### Response
```
{
  "success": true,
  "message": "Apply campaign successfully",
  "data": {
    "campaignId": "7d3f7c71-0c72-4a77-9d6f-5f6cb9d4d1a1",
    "code": "WELCOME50",
    "title": "Welcome New User",
    "isPercentage": true,
    "discountValue": 50,
    "maxDiscountAmount": 50000,
    "minOrderAmount": 100000,
    "message": "Apply campaign successfully"
  },
  "errors": null,
  "traceId": "0HNLMC2KQ88FJ:00000001",
  "timestampUtc": "2026-05-20T11:17:48.7978712Z"
}
```
### Auto apply campaign
```http
GET     /api/v1/campaigns/best
```
#### Request
```
"merchantId": "Guid"
```
#### Response
```
{
  "success": true,
  "message": "Get best campaign successfully",
  "data": {
    "campaignId": "c2d9d0a4-95d4-4e34-8b7f-9f5b9d8f3a22",
    "code": "FREESHIP",
    "title": "Free Ship",
    "isPercentage": false,
    "discountValue": 30000,
    "maxDiscountAmount": 30000,
    "minOrderAmount": 50000,
    "message": "Best campaign found"
  },
  "errors": null,
  "traceId": "0HNLMC2KQ88FJ:00000002",
  "timestampUtc": "2026-05-20T11:18:13.2286316Z"
}
```