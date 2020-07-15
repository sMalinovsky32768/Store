using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Store
{
    internal static class Security
    {
        
        public static bool SetUserCredentials(string login, string pass, int? userID = null)
        {
            using var connection = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            try
            {
                connection.Open();
                var isAvailable = Convert.ToBoolean(
                    new SqlCommand($"SELECT [dbo].[IsAvailableName](N'{login}')",
                    connection).ExecuteScalar());
                if (!isAvailable) return false;

                var byteConverter = new UnicodeEncoding();

                byte[] loginByte;
                byte[] passByte;
                string encryptedPassString;
                byte[] encryptedPass;
                byte[] keyForPass;
                byte[] ivForPass;

                loginByte = byteConverter.GetBytes(login);
                passByte = byteConverter.GetBytes(pass);
                using (var sha = SHA256.Create())
                {
                    keyForPass = sha.ComputeHash(loginByte);
                }

                using (var md5 = MD5.Create())
                {
                    ivForPass = md5.ComputeHash(passByte);
                }

                using (var myAes = new AesCryptoServiceProvider())
                {
                    myAes.Key = keyForPass;
                    myAes.IV = ivForPass;
                    encryptedPass = EncryptStringToBytes_Aes(pass, myAes.Key, myAes.IV);
                    encryptedPassString = Convert.ToBase64String(encryptedPass);
                }

                string commandString; 
                if (userID == null)
                {
                    commandString = $@"insert into [User] (Name, Password) values 
                                    (N'{login}', N'{encryptedPassString}')";
                }
                else
                {
                    commandString = $@"Update [User] set Name=N'{login}', 
                                    Password=N'{encryptedPassString}' where ID={userID}";
                }
                new SqlCommand(commandString, connection).ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
        }

        public static bool VerifyCredentials(string inLogin, string inPass)
        {
            using var connection = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            try
            {
                connection.Open();
                var isAvailable = Convert.ToBoolean(
                    new SqlCommand($"SELECT [dbo].[IsAvailableName](N'{inLogin}')", 
                    connection).ExecuteScalar());
                if (isAvailable) return false;

                var byteConverter = new UnicodeEncoding();

                byte[] encryptedPass;
                byte[] keyForPass;
                byte[] ivForPass;

                using (var sha = SHA256.Create())
                {
                    keyForPass = sha.ComputeHash(byteConverter.GetBytes(inLogin));
                }

                using (var md5 = MD5.Create())
                {
                    ivForPass = md5.ComputeHash(byteConverter.GetBytes(inPass));
                }

                using (var myAes = new AesCryptoServiceProvider())
                {
                    myAes.Key = keyForPass;
                    myAes.IV = ivForPass;

                    encryptedPass = EncryptStringToBytes_Aes(inPass, myAes.Key, myAes.IV);
                }
                var pass = new SqlCommand($"Select Top(1) Password from [User] where Name=N'{inLogin}'", 
                    connection).ExecuteScalar().ToString();
                return Convert.ToBase64String(encryptedPass) == pass;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
        }

        public static bool UpdateCredentials(int userID, string oldPass, string newPass, string newLogin = null)
        {
            using var connection = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            try
            {
                connection.Open();

                var byteConverter = new UnicodeEncoding();

                byte[] keyForPass;
                byte[] ivForPass;
                string decPass;

                var login = new SqlCommand($"Select Top(1) Name from [User] where ID={userID}",
                    connection).ExecuteScalar().ToString();
                var pass = new SqlCommand($"Select Top(1) Password from [User] where ID={userID}",
                    connection).ExecuteScalar().ToString();

                newLogin ??= login;
                using (var sha = SHA256.Create())
                {
                    keyForPass = sha.ComputeHash(byteConverter.GetBytes(login));
                }

                using (var md5 = MD5.Create())
                {
                    ivForPass = md5.ComputeHash(byteConverter.GetBytes(oldPass));
                }

                using (var myAes = new AesCryptoServiceProvider())
                {
                    myAes.Key = keyForPass;
                    myAes.IV = ivForPass;
                    decPass = DecryptStringFromBytes_Aes(Convert.FromBase64String(pass),
                        myAes.Key, myAes.IV);
                }

                if (oldPass == decPass)
                {
                    return SetUserCredentials(newLogin, newPass, userID);
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
        }

        public static int? GetUserID(string login)
        {
            using var connection = new SqlConnection(
               ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            try
            {
                connection.Open();
                int res = Convert.ToInt32(new SqlCommand(
                    $"select top(1) ID from [User] where Name=N'{login}'", 
                    connection).ExecuteScalar());
                return res;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
        }

        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            using (var aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    encrypted = msEncrypt.ToArray();
                }
            }

            return encrypted;
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (var aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
