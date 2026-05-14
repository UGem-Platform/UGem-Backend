# API CONTRACT - Food Topping

Base URL:

```txt
http://localhost:8080
```

---

# 1. Create Food Topping

Merchant thêm topping vào món ăn.

## Authorization

```txt
Bearer Token - Merchant
```

## Method

```http
POST /api/v1/food-toppings
```

## Request Body

```json
{
  "foodId": "09c2daa1-5f3e-45e7-9fc2-79938264c296",
  "name": "Them Trung",
  "price": 10000
}
```

## Request Fields

| Field | Type | Required | Description |
|---|---|---|---|
| foodId | Guid | true | Id của món ăn cần thêm topping |
| name | string | true | Tên topping |
| price | decimal | true | Giá topping |

## Response

```json
{
  "success": true,
  "message": "Create food topping successfully",
  "data": null,
  "errors": null,
  "traceId": "0HNLFB3CNC6BN:00000004"
}
```

---

# 2. Get Food Toppings By Food Id

Customer và Merchant lấy danh sách topping của một món ăn.

## Authorization

```txt
No required
```

## Method

```http
GET /api/v1/foods/{foodId}/toppings
```

## Request URL

```txt
http://localhost:8080/api/v1/foods/09c2daa1-5f3e-45e7-9fc2-79938264c296/toppings
```

## Path Params

| Field | Type | Required | Description |
|---|---|---|---|
| foodId | Guid | true | Id của món ăn |

## Response

```json
{
  "success": true,
  "message": "Get food toppings successfully",
  "data": [
    {
      "id": "10000000-0000-0000-0000-000000000001",
      "foodId": "09c2daa1-5f3e-45e7-9fc2-79938264c296",
      "name": "Them Trung",
      "price": 10000,
      "isActive": true
    }
  ],
  "errors": null,
  "traceId": "0HNLFB3CNC6BN:00000004"
}
```

---

# 3. Update Food Topping

Merchant chỉnh sửa topping. Field nào truyền lên thì update field đó.

## Authorization

```txt
Bearer Token - Merchant
```

## Method

```http
PUT /api/v1/food-toppings
```

## Request Body

```json
{
  "foodToppingId": "10000000-0000-0000-0000-000000000001",
  "name": "Them Trung Ga",
  "price": 12000,
  "isActive": true
}
```

## Request Fields

| Field | Type | Required | Description |
|---|---|---|---|
| foodToppingId | Guid | true | Id của topping cần sửa |
| name | string | false | Tên topping mới |
| price | decimal | false | Giá topping mới |
| isActive | bool | false | Trạng thái topping |

## Response

```json
{
  "success": true,
  "message": "Update food topping successfully",
  "data": null,
  "errors": null,
  "traceId": "0HNLFB3CNC6BN:00000004"
}
```

---

# 4. Delete Food Topping

Merchant xóa topping.

Lưu ý: Backend đang soft delete topping bằng cách set `IsDeleted = true` và `IsActive = false`.

## Authorization

```txt
Bearer Token - Merchant
```

## Method

```http
DELETE /api/v1/food-toppings/{foodToppingId}
```

## Request URL

```txt
http://localhost:8080/api/v1/food-toppings/10000000-0000-0000-0000-000000000001
```

## Path Params

| Field | Type | Required | Description |
|---|---|---|---|
| foodToppingId | Guid | true | Id của topping cần xóa |

## Response

```json
{
  "success": true,
  "message": "Delete food topping successfully",
  "data": null,
  "errors": null,
  "traceId": "0HNLFB3CNC6BN:00000004"
}
```

---

# Notes For Frontend

## Create topping

FE dùng API này cho Merchant thêm topping vào món.

```txt
POST /api/v1/food-toppings
```

## Show topping when customer orders food

FE gọi API này khi user mở chi tiết món hoặc chọn món.

```txt
GET /api/v1/foods/{foodId}/toppings
```

Sau đó render checkbox/list topping cho user chọn.

## Update topping

FE có thể gửi 1 field hoặc nhiều field.

Ví dụ chỉ sửa giá:

```json
{
  "foodToppingId": "10000000-0000-0000-0000-000000000001",
  "price": 15000
}
```

## Delete topping

FE gọi delete khi Merchant muốn xóa topping.

```txt
DELETE /api/v1/food-toppings/{foodToppingId}
```

---

# Need Update Later

Để tính bill đúng, backend cần sửa thêm:

```txt
POST /api/v1/orders
GET /api/v1/orders/bill
```

Order request cần thêm:

```json
{
  "foodId": "guid",
  "quantity": 2,
  "foodToppingIds": [
    "topping-guid-1",
    "topping-guid-2"
  ]
}
```

Công thức tính tiền:

```txt
SubTotal = (Food.Price + TotalToppingPrice) * Quantity
```
