# Tài liệu nhanh: RedirectServer (PACS & ShortLink)

Tài liệu này mô tả nhanh cách cấu hình và sử dụng hai phần chính của dự án:
- Dịch vụ PACS (tạo link unencrypted / encrypted)
- Dịch vụ ShortLink (rút gọn URL)

Nội dung chính: cấu hình, biến môi trường, các endpoint, ví dụ curl, và cách đăng ký DI.

---

## 1. Mô tả chung
- Dự án là một API nhỏ cho phép tạo link tới hệ thống PACS (có thể ở dạng plaintext hoặc encrypted token) và một dịch vụ rút gọn link (shortlink).
- Các controller chính:
  - `PacsLinkController` (route base: `/api/PacsLink`) — exposes endpoints để tạo link PACS.
  - `ShortLinkController` — endpoints để tạo/tra cứu/redirect shortlink.

## 2. Cấu hình (appsettings / env)
Các cấu hình chính nằm ở `appsettings.json` dưới khóa `PacsClient`:
- `PacsClient:BaseUrl` – URL gốc của PACS portal (ví dụ: `http://pacs.example.com/portal`).
- `PacsClient:ImagePath` – path mà hình ảnh hoặc view được đặt (ví dụ: `portal`).
- `PacsClient:EncryptPath` – endpoint (relative) để gọi API encryption/token (nếu cần).
- `PacsClient:DefaultExpirationDays` – (tùy chọn) số ngày mặc định cho expiration.

Ví dụ (từ `appsettings.json`):
```
"PacsClient": {
    "BaseUrl": "http://pacs.benhvienungbuou.vn:8081/portal",
    "EncryptPath": "portal/CSPublicQueryService/CSPublicQueryService.svc/json/EncryptQSSecure?embed_cred=1",
    "ImagePath": "portal"
}
```

Biến môi trường quan trọng (thường đặt trong `launchSettings.json` cho dev):
- `KEY` – khóa dùng cho giải mã XOR+Base64 nếu mã hóa input được dùng.
- `USERNAME` – username để đưa vào query tới PACS.
- `PASSWORD` – password tương ứng.
- `DB_CONNECTION_STRING` – chuỗi kết nối DB.

## 3. Các endpoint

### 3.1 PACS links (`PacsLinkController`) — base: `/api/PacsLink`
- GET `/api/PacsLink/unencrypted?input={rawQuery}`
  - Mô tả: tạo link chưa mã hóa (dùng trực tiếp các cặp query được cung cấp).
  - Tham số `input`: chuỗi query (ví dụ `studyUID=1&hide_report=1`) hoặc dữ liệu đã được mã hóa tùy implement.
  - Trả về: object JSON `PacsLink` có ít nhất `Url` và `Message`.

- GET `/api/PacsLink/encrypted?input={rawQuery}`
  - Mô tả: tạo link encrypted: api sẽ gọi PACS token service (qua `IPacsClient`) để lấy token rồi đóng gói vào URL.
  - Trả về: `PacsLink` với `Url`, `Token`, `Message`.

Ví dụ truy vấn (curl):
```bash
curl "http://localhost:5098/api/PacsLink/unencrypted?input=studyUID=1&hide_report=1"

curl "http://localhost:5098/api/PacsLink/encrypted?input=studyUID=1&hide_report=1"
```

> Lưu ý: `input` phải được url-encode khi chứa ký tự đặc biệt.

### 3.2 ShortLink (`ShortLinkController`)
- POST `/shortlinks`  (body JSON)
  - Body: `{ "OriginalUrl": "https://example.com/full/path" }`
  - Trả về: `{ shortUrl, code }` (shortUrl là URL đầy đủ tới redirect endpoint).

- GET `/shortlinks/{code}`
  - Mô tả: lấy thông tin meta (originalUrl, clicks, createdAt).

- GET `/{code}`
  - Mô tả: redirect (HTTP 302) tới `OriginalUrl`.

Ví dụ (curl):
```bash
curl -X POST "http://localhost:5098/shortlinks" -H "Content-Type: application/json" -d '{"OriginalUrl":"https://example.com/foo"}'
```

## 4. Dạng `input` cho PACS
- `input` mong đợi là một chuỗi query-like gồm cặp `key=value` phân tách bởi `&`.
- Ví dụ hợp lệ: `studyUID=1&seriesUID=2&hide_report=1`

## 5. Triển khai & chạy (dev)
- Mặc định config trong `Properties/launchSettings.json` có profile `http` port = `5098`.
- Run bằng Visual Studio / `dotnet run` từ thư mục `RedirectServer`.

Kiểm tra nhanh:
```bash
curl http://localhost:5098/swagger  # xem swagger nếu đã bật
curl "http://localhost:5098/api/PacsLink/unencrypted?input=studyUID=1"
```

