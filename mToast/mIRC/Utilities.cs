using System;
using System.Runtime.InteropServices;

namespace MircSharp
{
    public static class Extensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    public class Utilities
    {
        public static string GetData(ref IntPtr data)
        {
            return Marshal.PtrToStringUni(data);
        }

        public static void SetData(ref IntPtr data, string output)
        {
            IntPtr pData = IntPtr.Zero;

            pData = Marshal.StringToHGlobalUni(output);
            NativeMethods.MemCopy(data, pData, (uint)(output.Length + 1) * sizeof(char));

            Marshal.FreeHGlobal(pData);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
