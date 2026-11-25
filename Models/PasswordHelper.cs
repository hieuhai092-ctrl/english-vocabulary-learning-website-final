using System.Security.Cryptography;
using System.Text;

namespace English_Vocabulary_Learning_Website.Models
{
    public static class PasswordHelper
    {
        // Chuỗi "muối" cố định của ứng dụng. Đã dùng để băm rồi thì KHÔNG đổi nữa,
        // nếu đổi thì mọi mật khẩu đang lưu sẽ không đăng nhập được.
        private const string AppSalt = "EnglishVocab@2026";

        public static string Hash(string username, string password)
        {
            string input = (username ?? "").Trim().ToLower() + ":" + (password ?? "") + ":" + AppSalt;

            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();   // chuỗi 64 ký tự
            }
        }
    }
}