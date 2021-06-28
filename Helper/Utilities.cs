using System;
using System.Text;

namespace FaceLivenessDetection
{
    public static class Utilities
    {
        public static string EncodeCredentials(string userName, string password)
        {
            return Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes($"{userName}:{password}"));
        }
    }
}
