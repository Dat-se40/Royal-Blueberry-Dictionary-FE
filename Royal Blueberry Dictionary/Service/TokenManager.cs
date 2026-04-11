using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service
{
    public static class TokenManager
    {
        private static string _jwtToken;

        // Lưu vào RAM (App tắt là mất đăng nhập - dễ làm nhất)
        public static void SaveToken(string token)
        {
            _jwtToken = token;
        }

        public static string GetToken()
        {
            return _jwtToken;
        }

        public static void ClearToken()
        {
            _jwtToken = null;
        }

        // Lưu ý pro: Nếu bạn đang làm WPF và muốn tắt app mở lại vẫn CÒN đăng nhập, 
        // hãy lưu vào Properties.Settings.Default thay vì biến static.
        // Nếu làm MAUI, hãy dùng SecureStorage.SetAsync("jwt", token).
    }
}
