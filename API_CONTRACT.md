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
POST /api/v1/admins
GET /api/v1/admins
GET /api/v1/admins/{id}
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
            "id": "be6aca0a-ac95-4efd-9f21-b686aa56b193",
            "status": "Rejected",
            "motivation": "string",
            "experience": "string",
            "facebookUrl": "string",
            "tiktokUrl": "string",
            "youtubeUrl": "string",
            "otherSocialUrl": "string",
            "rejectionReason": "string",
            "customerId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
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

## 13. FRONTEND ARCHITECTURE SUGGESTION

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
## 14. REVIEWER APPLICATIONS

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

## 15. USERS

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
