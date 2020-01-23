using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Trail365.Internal;

namespace Trail365
{
    public class HashUtils
    {
        public static string CalculateHash(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            Guard.Assert(stream.Position == 0);
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(stream);
                var sBuilder = new StringBuilder();
                foreach (byte item in data)
                {
                    sBuilder.Append(item.ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static string CalculateHash(byte[] buffer)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(buffer);
                var sBuilder = new StringBuilder();
                foreach (byte item in data)
                {
                    sBuilder.Append(item.ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static string CalculateHash(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);
            return CalculateHash(data);
        }
    }
}
