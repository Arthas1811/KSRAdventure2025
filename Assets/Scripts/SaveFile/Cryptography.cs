using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
 
namespace AG_NameSpace
{
    public static class StringCipher
    {
        const string passPhrase = "Passphrase hier einfüllen";
 
        private const int KeySize = 256;
        private const int DerivationIterations = 1000;
 
        public static string Encrypt(string plainText)
        {
            byte[] salt = GenerateRandomBytes(32);
            byte[] iv = GenerateRandomBytes(16); // AES block size = 128-bit IV
 
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
 
            using var keyDerivation = new Rfc2898DeriveBytes(passPhrase, salt, DerivationIterations, HashAlgorithmName.SHA256);
            byte[] key = keyDerivation.GetBytes(KeySize / 8);
 
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
 
            using MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
            }
 
            byte[] cipherBytes = ms.ToArray();
 
            byte[] result = salt
                .Concat(iv)
                .Concat(cipherBytes)
                .ToArray();
 
            return Convert.ToBase64String(result);
        }
 
        public static string Decrypt(string cipherText)
        {
            byte[] fullBytes = Convert.FromBase64String(cipherText);
 
            byte[] salt = fullBytes.Take(32).ToArray();
            byte[] iv = fullBytes.Skip(32).Take(16).ToArray();
            byte[] cipherBytes = fullBytes.Skip(48).ToArray();
 
            using var keyDerivation = new Rfc2898DeriveBytes(passPhrase, salt, DerivationIterations, HashAlgorithmName.SHA256);
            byte[] key = keyDerivation.GetBytes(KeySize / 8);
 
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
 
            using MemoryStream ms = new MemoryStream(cipherBytes);
            using CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader sr = new StreamReader(cs, Encoding.UTF8);
 
            return sr.ReadToEnd();
        }
 
        private static byte[] GenerateRandomBytes(int size)
        {
            byte[] bytes = new byte[size];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }
 
 
    }
}