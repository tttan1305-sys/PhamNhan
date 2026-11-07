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

## Bản Unity (giai đoạn 1)

`unity/PhamNhan/` là dự án Unity 6 (6000.0.84f1). Giai đoạn 1 chuyển màn công thành sang C# để chơi trên điện thoại Android, có điều khiển cảm ứng.

| Đường dẫn | Nội dung |
|---|---|
| `Assets/Scripts/Sim/` | Lõi mô phỏng trận bằng C# thuần, không phụ thuộc Unity: quân, tướng, quân sư AI, trận nhãn, kết giới, chiêu thức. |
| `Assets/Scripts/View/` | Phần Unity: vẽ chiến trường, máy quay, giao diện, cần gạt và nút cảm ứng, màn hình chính. |
| `Assets/Editor/BuildScript.cs` | Menu **Phàm Nhân → Build Android APK**, cũng là hàm GitHub Actions gọi để build. |
| `unity/SimTest/` | Chạy thử trận ở chế độ tự đánh không cần Unity: `dotnet run --project unity/SimTest -- 900`. |

**Mở trong Unity**: thêm thư mục `unity/PhamNhan` vào Unity Hub rồi bấm Play. Trò chơi tự khởi động, không cần kéo thả gì vào cảnh.

**Build tự động**: mỗi lần đẩy lên `main`, quy trình `.github/workflows/android.yml` build APK và đăng lên trang Releases, mục *ban-thu-nghiem*. Cần ba mục bí mật của repo là `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_LICENSE`; khi thiếu, quy trình bỏ qua bước build.

**Âm thanh**: `Assets/Scripts/Audio/Synth.cs` tổng hợp 41 tiếng động và 3 bản nhạc nền (màn hình chính, trong trận, cao trào) bằng mã lúc mở game, không dùng file âm thanh. Lõi trận phát sự kiện qua `Siege.snd`, `View/Sound.cs` phát theo khoảng cách tới máy quay. Nút bật/tắt nhạc và âm thanh ở màn hình chính và khi tạm dừng.

**Tạo hình nhân vật**: `Assets/Scripts/Art/` vẽ 10 tướng và 8 binh chủng bằng mã (C# thuần, không dùng file ảnh). Mỗi loại có 4 khung: đứng, hai bước đi, ra đòn. Ảnh dựng ở luồng phụ lúc mở game (`View/CharSprites.cs`). Màn hình chính có mục **Nhân vật** để xem từng tạo hình. Xem thử không cần Unity: `dotnet run --project unity/ArtTest -- bang.png 160` ghi bảng tạo hình ra PNG.
