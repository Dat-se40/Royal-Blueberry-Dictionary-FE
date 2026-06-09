# Royal BlueBerry Dictionary 📚

## Tổng quan đồ án

Royal BlueBerry Dictionary là phiên bản nâng cấp toàn diện của ứng dụng từ điển tiếng Anh được xây dựng bằng WPF .NET. Ứng dụng giờ đây hoạt động theo mô hình Client-Server với hệ thống tài khoản, đồng bộ đám mây, các gói từ vựng offline và đặc biệt tích hợp tính năng học từ vựng qua trò chơi (Minigames).

### Thông tin đồ án

* **Môn học**: Nhập môn công nghệ phần mềm
* **Giảng viên**: TS.Huỳnh Ngọc Tin
* **Học kỳ**: 2 - 2025-2026
* **Thành viên nhóm**:
* Nguyễn Tấn Đạt
* Võ Nguyễn Thanh Hương
* Võ Văn Hải
* Ngô Phương Hiền
* Nguyễn Quốc An



### Công nghệ

* **Framework**: WPF .NET
* **Pattern**: MVVM (sử dụng `CommunityToolkit.Mvvm`)
* **Kiến trúc**: Dependency Injection, Repository Pattern
* **Kết nối**: RESTful API Client (Giao tiếp với Backend Server)
* **Database Local**: Entity Framework Core (`AppDbContext`)

### Build .exe file

```bash
dotnet clean
dotnet publish -c Release -r win-x64 --self-contained true
cd bin\Release\net9.0-windows\win-x64\publish
.\RoyalBlueBerryDictionary.exe
```

---

## Chức năng chính

### 1. Hệ thống Tài khoản & Đồng bộ (Mới 🌟)

* **Xác thực**: Hỗ trợ đăng nhập/đăng ký tài khoản hệ thống (`LoginRequest`, `RegisterRequest`).
* **Google Login**: Tích hợp đăng nhập nhanh qua Google (`GoogleLoginRequest`).
* **Quản lý Token**: Tự động refresh token và duy trì phiên đăng nhập (`TokenManager`).
* **Đồng bộ**: Sử dụng ứng dụng trên nhiều thiết bị mà không mất dữ liệu cá nhân.

### 2. Tra cứu & Quản lý từ vựng nâng cao

* **Backend API Integration**: Dữ liệu từ vựng được quản lý tập trung thông qua Backend server.
* **My Words & Favourite Words**: Quản lý kho từ vựng cá nhân.
* **Hệ thống Tags (Nhãn)**: Tạo và gán nhãn cho từ vựng để dễ dàng phân loại (`TagPickerDialog`, `TagService`).
* **History & Caching**: Lưu lịch sử tìm kiếm và cache từ vựng (`CachedWord`) giúp tăng tốc độ trải nghiệm.

### 3. Học tập qua Trò chơi (Minigames 🎮)

* Trải nghiệm học từ vựng không nhàm chán với các module trò chơi (`GamePage.xaml`).
* **Lịch sử & Thống kê**: Theo dõi tiến độ và lịch sử hoàn thành trò chơi (`GameLogService`, `GameHistoryDialog.xaml`).

### 4. Gói từ vựng (Packages 📦)

* **Offline Packages**: Cung cấp các gói từ vựng theo chủ đề (IELTS, TOEIC...).
* Có thể tải xuống và lưu trữ cache offline để học bất cứ lúc nào (`OfflinePackageCacheService`, `PackageCard.xaml`).

### 5. Giao diện & Cá nhân hóa

* **Theme Manager**: Hỗ trợ thay đổi giao diện linh hoạt (Sáng/Tối) và lưu Presets (`ThemeManeger.cs`, `ThemePresetDialog.xaml`).
* **Custom Fonts & Colors**: Người dùng tự do tùy chỉnh phông chữ, màu sắc ứng dụng (`CustomThemeDialog.xaml`, `FontPickerDialog.xaml`).
* **Responsive & Animations**: Thiết kế mượt mà, phản hồi tốt trên nhiều độ phân giải màn hình.
---

## Cách sử dụng + User Flow

### Flow 1: Khởi động & Đăng nhập

```text
1. Mở ứng dụng (Hiển thị SplashWindow)
2. Nếu chưa có phiên đăng nhập -> Chuyển đến WelcomePage
3. Chọn Đăng nhập (Tài khoản hệ thống hoặc Google) / Đăng ký
4. Token được cấp phát và lưu bởi TokenManager
5. Chuyển vào HomePage
```

### Flow 2: Trải nghiệm Minigame Học từ

```text
1. Điều hướng đến GamePage từ Sidebar
2. Chọn bộ từ vựng / Tag muốn ôn tập (có thể chỉnh trong GameSettingsDialog)
3. Chơi game đoán nghĩa/phát âm
4. Kết thúc game -> Hiện GameCompletionDialog (Thống kê điểm số)
5. Lịch sử được lưu vào hệ thống qua GameLogService
```

### Flow 3: Quản lý Gói từ vựng Offline

```text
1. Vào mục Offline Packages
2. Duyệt các gói từ có sẵn (PackageCard)
3. Click để xem chi tiết (PackageDetailsDialog)
4. Nhấn Tải về (hệ thống xử lý qua OfflinePackageCacheService)
5. Sử dụng gói từ offline ngay cả khi mất mạng internet
```

---

## Cách cài đặt

### Yêu cầu hệ thống

* Windows 10/11
* .NET 8.0/9.0 SDK & Runtime
* Visual Studio 2022

### Cài đặt từ Source Code

1. **Clone repository**

```bash
git clone https://github.com/Dat-se40/royal-blueberry-dictionary-fe.git
cd royal-blueberry-dictionary-fe
```

2. **Cấu hình API & Backend Server**
* Đảm bảo Backend Server (đồ án phần backend) đang chạy.
* Mở file `Royal Blueberry Dictionary/appsettings.json`
* Cập nhật thông tin endpoint của backend:



```json
{
  "ApiSettings": {
    "BaseUrl": "https://your-backend-api-url.com/"
  }
}
```

3. **Restore & Build**

```bash
dotnet restore
dotnet build
dotnet run --project "Royal Blueberry Dictionary"
```

---

## Tổng quan Kiến trúc & Kỹ Thuật

Kiến trúc đã được refactor mạnh mẽ để tuân thủ **Dependency Injection (DI)** và sử dụng **CommunityToolkit.Mvvm**.

### Cấu trúc Project

```text
Royal Blueberry Dictionary/
├── Config/             # Lớp cấu hình (ApiSettings.cs)
├── Database/           # SQLite/EF Core Context (AppDbContext.cs)
├── Documents/          # Tài liệu hướng dẫn DI & MVVM Toolkit
├── Model/              # Models chia theo domain (Auth, Game, Settings, Word, Package)
├── Repository/         # Interface & Impl cho truy xuất dữ liệu cục bộ (TagRepository...)
├── Service/            # Logic nghiệp vụ & API Client
│   ├── ApiClient/      # Xử lý gọi Backend API (BackendApiClient.cs)
│   ├── AuthService.cs  # Xử lý xác thực
│   ├── GameLogService.cs
│   └── TokenManager.cs # Quản lý Access/Refresh token
├── View/               # Lớp UI
│   ├── Dialogs/        # Các popup (Settings, MeaningSelector, NoteWriter...)
│   ├── Pages/          # Các trang chính (Account, Game, History, OfflinePackages...)
│   └── UserControl/    # Component tái sử dụng (WordCard, PackageCard...)
├── ViewModel/          # Chứa logic ViewModels kế thừa từ ObservableObject
└── Resources/          # Resource Dictionaries (Colors, ButtonStyles, ControlStyles)
```

---

## Kiến trúc Chi tiết

### 1. Quản lý State & Dependency Injection (DI)

Các service được đăng ký trong `App.xaml.cs` (hoặc lớp Startup):

```csharp
// Đăng ký Services & ViewModels (Ví dụ)
services.AddSingleton<IBackendApiClient, BackendApiClient>();
services.AddSingleton<TokenManager>();
services.AddSingleton<AuthService>();
services.AddTransient<GameViewModel>();
```

### 2. Mô hình MVVM với Community Toolkit (`MvvmToolkit.md`)

Sử dụng Source Generators để giảm boilerplate code:

```csharp
public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchQuery;

    private readonly SearchService _searchService;

    public SearchViewModel(SearchService searchService)
    {
        _searchService = searchService;
    }

    [RelayCommand]
    private async Task PerformSearchAsync()
    {
        // Logic tìm kiếm gọi API...
    }
}
```

### 3. Tương tác API (`BackendApiClient`)

Mọi giao tiếp mạng đều được gom gọn qua Interface `IBackendApiClient` và quản lý response qua `ApiResponse<T>`:

```csharp
public async Task<ApiResponse<GoogleLoginResponse>> GoogleLoginAsync(GoogleLoginRequest request)
{
    // Đính kèm token và gửi HTTP POST tới backend...
}
```

### 4. Quản lý Theme & Giao diện (`ThemeManeger`)

Hỗ trợ Dynamic Resources và Presets:

```csharp
// Chuyển đổi theme linh hoạt dựa trên cấu hình AppSettings
public void ApplyColorTheme(AppColorTheme theme)
{
    // Clear old dictionaries and merge new ones
}
```

---

## Best Practices được áp dụng

1. **Dependency Inversion**: Các ViewModel không tự tạo Service (không dùng `new Service()`), mà nhận qua Constructor Injection.
2. **Repository Pattern**: Tách biệt logic truy cập database (thẻ tag, từ đã lưu local) qua `ITagRepository`, `IWordEntryRepository`.
3. **Mvvm Toolkit**: Tận dụng tối đa `[ObservableProperty]` và `[RelayCommand]` thay vì implement `INotifyPropertyChanged` và `ICommand` thủ công.
4. **Quản lý lỗi tập trung**: Định nghĩa chung `ApiErrorResponse` để handle lỗi từ backend trả về một cách thống nhất trên UI.

---

## Contact

* **Email**: 24520280@gm.uit.edu.vn
* **GitHub**: [https://github.com/Dat-se40/Royal-Blueberry-Dictionary-FE](https://github.com/Dat-se40/Royal-Blueberry-Dictionary-FE))

---

## Acknowledgments

* Microsoft MVVM Community Toolkit
* Tác giả các thư viện mã nguồn mở được dùng trong dự án
* TS.Huỳnh Ngọc Tín & UIT
