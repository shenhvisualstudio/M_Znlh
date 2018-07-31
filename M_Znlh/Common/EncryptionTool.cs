using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using ServiceInterface.Common;
using System.Text;
using System.IO;

namespace ServiceInterface.Common
{
    public static class DES_IV
    {
        //DES加密CBC模式初始化向量值
        public const string IV = "ndR+2cS7";
    }

    public class EncryptionTool
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="strText">待加密字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string MD5_Encrypt(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strText));          
            return BitConverter.ToString(result).Replace("-", "");
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="strKey">秘钥</param>
        /// <param name="strText">待加密字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string DES_Encrypt(string strKey, string strText)
        {
            string password2 = DES_IV.IV;
            string password = strKey;
            string ciphertext = strText;

            char[] key = new char[8];
            if (password.Length > 8)
            {
                password = password.Remove(8);
            }
            password.CopyTo(0, key, 0, password.Length);

            char[] iv = new char[8];
            if (password2.Length > 8)
            {
                password2 = password2.Remove(8);
            }
            password2.CopyTo(0, iv, 0, password2.Length);

            ciphertext = ciphertext.Replace(" ", "+");
            SymmetricAlgorithm serviceProvider = new DESCryptoServiceProvider();
            serviceProvider.Key = Encoding.ASCII.GetBytes(key);
            serviceProvider.IV = Encoding.ASCII.GetBytes(iv);

            byte[] contentArray = Convert.FromBase64String(ciphertext);
            MemoryStream memoryStream = new MemoryStream(contentArray);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, serviceProvider.CreateDecryptor(), CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);

            string strEncryptText = streamReader.ReadToEnd();

            streamReader.Dispose();
            cryptoStream.Dispose();
            memoryStream.Dispose();
            serviceProvider.Clear();

            return strEncryptText;
        }
    }
}