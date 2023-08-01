using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Data.Entity;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DataAccess
{
   
    public class CryptSHA256 
    {
        public static string Decrypt(string encrypted)
        {
            if (encrypted == null || string.IsNullOrEmpty(encrypted))
                return "";

            byte[] data = System.Convert.FromBase64String(encrypted);
            byte[] rgbKey = System.Text.ASCIIEncoding.ASCII.GetBytes("12121212");
            byte[] rgbIV = System.Text.ASCIIEncoding.ASCII.GetBytes("34343434");

            MemoryStream memoryStream = new MemoryStream(data.Length);

            DESCryptoServiceProvider desCryptoServiceProvider = new
            DESCryptoServiceProvider();

            CryptoStream cryptoStream = new CryptoStream(memoryStream,
            desCryptoServiceProvider.CreateDecryptor(rgbKey, rgbIV),
            CryptoStreamMode.Read);

            memoryStream.Write(data, 0, data.Length);

            memoryStream.Position = 0;

            string decrypted = new StreamReader(cryptoStream).ReadToEnd();

            cryptoStream.Close();

            return decrypted;

        }
        public static string Encrypt(string decrypted)
        {
            if (string.IsNullOrEmpty(decrypted))
                return "";

            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(decrypted);

            byte[] rgbKey = System.Text.ASCIIEncoding.ASCII.GetBytes("12121212");

            byte[] rgbIV = System.Text.ASCIIEncoding.ASCII.GetBytes("34343434");

            MemoryStream memoryStream = new MemoryStream(1024);

            DESCryptoServiceProvider desCryptoServiceProvider = new
            DESCryptoServiceProvider();

            CryptoStream cryptoStream = new CryptoStream(memoryStream,
            desCryptoServiceProvider.CreateEncryptor(rgbKey, rgbIV),
            CryptoStreamMode.Write);

            cryptoStream.Write(data, 0, data.Length);

            cryptoStream.FlushFinalBlock();

            byte[] result = new byte[(int)memoryStream.Position];

            memoryStream.Position = 0;

            memoryStream.Read(result, 0, result.Length);

            cryptoStream.Close();

            return System.Convert.ToBase64String(result);

        }
    }
}
