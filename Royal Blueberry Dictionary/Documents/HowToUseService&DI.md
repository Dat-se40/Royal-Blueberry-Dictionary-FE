# 📘 Hướng dẫn sử dụng Service & Dependency Injection (DI)

Tài liệu này hướng dẫn cách phát triển và sử dụng các Service trong dự án **Royal Blueberry Dictionary**.

## 1. Cơ chế Dependency Injection (DI) trong `App.xaml.cs`

Chúng ta sử dụng thư viện `Microsoft.Extensions.DependencyInjection` để quản lý các đối tượng. Thay vì tự khởi tạo `new Service()`, chúng ta đăng ký chúng trong `App.xaml.cs`.
### Injection Dependency là gì? 
Dependency Injection (DI) là một kỹ thuật trong lập trình giúp quản lý sự phụ thuộc giữa các đối tượng. Thay vì một lớp tự tạo ra các đối tượng mà nó cần, DI cho phép chúng ta "tiêm" các đối tượng này từ bên ngoài vào thông qua constructor, phương thức hoặc thuộc tính. Điều này giúp tăng tính linh hoạt, dễ bảo trì và kiểm thử của ứng dụng.

Các interface và implementation được đăng ký trong `App.xaml.cs` sẽ được quản lý bởi một **Service Container**. Khi một Service được yêu cầu, container sẽ tạo ra một instance mới hoặc trả về instance đã tồn tại dựa trên cách chúng ta đã đăng ký.   

### Các kiểu đăng ký (Service Lifetimes):
Trong `OnStartup`, chúng ta sử dụng:
* **AddSingleton**: Tạo một thực thể duy nhất dùng cho toàn bộ vòng đời ứng dụng (ví dụ: `ApiClient`, `ApiSettings`).
* **AddScoped**: (Trong WPF thường tương đương Transient) Tạo một thực thể mới khi được yêu cầu trong một phạm vi (thường dùng cho `DbContext` ,`Repository`, `Service`).

### Ví dụ đăng ký trong `App.xaml.cs`:
```csharp
// Đăng ký Service vào Container
serviceCollection.AddSingleton<IBackendApiClient, BackendApiClient>();
serviceCollection.AddScoped<Service.SearchService>(); 
```

---

## 2. Cách sử dụng Service từ UI (MainWindow/Pages)

Để lấy một Service đã đăng ký, chúng ta truy cập thông qua `App.serviceProvider`.

**Ví dụ lấy `SearchService` trong `MainWindow.xaml.cs`:**
```csharp
public MainWindow()
{
    InitializeComponent();
    // Lấy service từ container bằng GetRequiredService<Tên class muốn lấy> thay vì 'new'
    var searchService = App.serviceProvider.GetRequiredService<Service.SearchService>();
}
```

---

## 3. Lập trình bất đồng bộ (Async/Await) & Task

Khi gọi API hoặc truy vấn Database, chúng ta **bắt buộc** dùng bất đồng bộ để không làm "đơ" giao diện người dùng (UI Thread).

### Khái niệm cơ bản:
* **Task**: Đại diện cho một công việc đang được thực thi.
* **async**: Đánh dấu một phương thức là bất đồng bộ.
* **await**: Đợi kết quả trả về mà không làm chặn luồng chính.

### Cách dùng trong Service:
```csharp
// Phương thức trả về Task<WordDetail> thay vì WordDetail
public async Task<WordDetail> searchAWord(string word)
{
    // Sử dụng await để đợi kết quả từ API
    var response = await backendApiClient.GetAsync<WordDetail>($"searching/get-detail/{word}");
    return response;    
}
```

### Cách gọi từ UI (Sử dụng cho sự kiện Click):
```csharp
private async void btnSearch_Click(object sender, RoutedEventArgs e)
{
    var searchService = App.serviceProvider.GetRequiredService<SearchService>();
    
    // UI vẫn mượt mà trong khi đợi kết quả
    WordDetail result = await searchService.searchAWord("hello"); 
    
    txtDefinition.Text = result.Meanings[0].Definitions[0].Text;
}
```

---

## 4. Quy trình tạo một Service mới
1.  **Định nghĩa Interface** (nếu cần) trong thư mục `Service/Interface`.
2.  **Viết Implementation** trong thư mục `Service`.
3.  **Đăng ký** trong `App.xaml.cs` bằng `serviceCollection.AddScoped<YourService>();`.
4.  **Inject** vào nơi cần dùng thông qua `GetRequiredService`.

---
> **Lưu ý:** Luôn kiểm tra `appsettings.json` để đảm bảo `BaseUrl` của API chính xác trước khi chạy ứng dụng.

---
