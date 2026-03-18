# Observable Object
Cho 1 class kế thừa từ `ObservableObject`, bạn có thể sử dụng thuộc tính `SetProperty` để tự động thông báo khi giá trị của một thuộc tính thay đổi. Điều này giúp cập nhật giao diện người dùng (UI) một cách hiệu quả khi dữ liệu thay đổi.

`[ObservaleObject]` types cho phép tạo các biến public từ các biến private:
```csharp
	public class MyViewModel : ObservableObject
{
// Nếu không có [ObservableProperty]
	private string _myProperty;
	public string MyProperty
	{
		get => _myProperty;
		set => SetProperty(ref _myProperty, value);
	}
	// Nếu có
	[ObservableProperty]
	public partial class MyViewModel : ObservableObject
	{
		[ObservableProperty]
		private string _myProperty; // Bên ngoài sẽ tự động tạo ra public property MyProperty với getter và setter sử dụng SetProperty
	}
}
```