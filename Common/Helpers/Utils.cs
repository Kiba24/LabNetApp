﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Common.Helpers
{
    public static class Utils
    {
        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = DateTime.UtcNow;
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static String PassEncrypt(string str)
        {
            string passEncriptada = BCrypt.Net.BCrypt.HashPassword(str);
            return passEncriptada;
        }

        public static bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, hashedPassword);
        }

        public static bool IsValidPassword(string password)
        {
            var validationAttribute = new ValidPasswordAttribute();
            return validationAttribute.IsValid(password);
        }

    }


}
