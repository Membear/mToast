using System;
using System.Runtime.InteropServices;

namespace MircSharp
{
    internal static class Extensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }

            return value;
        }
    }

    /// <summary>
    /// Utility functions for mIRC
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Retrieves string from mIRC data pointer
        /// </summary>
        /// <param name="data">mIRC data pointer</param>
        /// <returns>String sent from mIRC</returns>
        public static string GetData(ref IntPtr data)
        {
            return Marshal.PtrToStringUni(data);
        }

        /// <summary>
        /// Passes string to mIRC through data pointer
        /// </summary>
        /// <param name="data">mIRC data pointer</param>
        /// <param name="input">string to pass</param>
        public static void SetData(ref IntPtr data, in string input)
        {
            IntPtr pData = IntPtr.Zero;

            pData = Marshal.StringToHGlobalUni(input);
            NativeMethods.MemCopy(data, pData, (uint)(input.Length + 1) * sizeof(char));

            Marshal.FreeHGlobal(pData);
        }

        /// <summary>
        /// Base64 encodes text for use with mIRC's $unsafe() or $decode()
        /// </summary>
        /// <param name="plainText">Plain text</param>
        /// <returns>Base64 encoded text</returns>
        public static string Base64Encode(in string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
