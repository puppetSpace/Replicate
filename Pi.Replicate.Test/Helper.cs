using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pi.Replicate.Test
{
    internal static class Helper
    {

        public static string CreateBase64HashForFile(string path)
        {
            MD5 hashCreator = MD5.Create();
            var hash = hashCreator.ComputeHash(System.IO.File.ReadAllBytes(path));
            return Convert.ToBase64String(hash);
        }
    }
}
