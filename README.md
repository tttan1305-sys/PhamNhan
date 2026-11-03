# Phàm Nhân

Game chiến thuật tu tiên thời gian thực (mobile + web, dự kiến làm bằng Unity). Repo này chứa các bản chơi thử HTML và bản vẽ giao diện ở giai đoạn thiết kế.

## Thư mục

| Đường dẫn | Nội dung |
|---|---|
| `cong-thanh/index.html` | Màn công thành Phù Vân, bản 7 (mới nhất). Mở trực tiếp bằng trình duyệt là chơi được. |
| `cong-thanh/index-v4.html`, `index-v5.html` | Bản 4 (màu thủy mặc tối) và bản 5 (màu sáng) để so sánh. |
| `cong-thanh/src/` | Mã nguồn dạng từng phần của màn công thành và script dựng. |
| `prototype/index.html` | Prototype bản đồ thế giới. |
| `giao-dien/` | Canvas 8 màn giao diện. |
| `anh-chup/` | Ảnh chụp các màn giao diện. |

## Chơi màn công thành

Mở `cong-thanh/index.html`.

**Phím điều khiển**

| Phím | Tác dụng |
|---|---|
| WASD | Di chuyển |
| J | Chém |
| 1 2 3 4 | Kỹ năng |
| Shift | Độn |
| T | Bày trận |
| F | Phá trận phù |
| G | Trấn sơn ấn |
| H | Uống đan |
| Q E R | Chọn thế trận (tự quyết, phân binh, hợp binh) |
| M | Mở sa bàn |

**Tham số URL**
- `?demo=1`: nhân vật tự đánh.
- `?rec=1`: bật `__step(n)` và `__sim(giây)` để quay video bằng Playwright.

## Dựng lại từ mã nguồn

```bash
bash cong-thanh/src/build6.sh   # tạo cong-thanh/src/index.html
```

Script chạy lần lượt ba bước:
1. `v4/build.py` ghép `index_v3.html` với các phần trong `v4/`.
2. `v5/theme.py` đổi bảng màu.
3. `v6/icons.py` thêm biểu tượng chiêu thức và sửa bố cục chữ.

Video quay các bản (file `.mp4`, tổng khoảng 330 MB) không đưa lên repo vì quá nặng.
