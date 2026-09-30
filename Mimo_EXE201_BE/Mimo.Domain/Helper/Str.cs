using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimo.DomainLayer.Helpers
{
    public static class StrHelper
    {
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Shared.Next(chars.Length)]).ToArray());
        }

        public static string GenerateRandomOTP(int length = 6)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Shared.Next(chars.Length)]).ToArray());
        }
    }
}
