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
            return Marshal.PtrToStringAnsi(data);
        }

        public static void SetData(ref IntPtr data, string output)
        {
            IntPtr pData = IntPtr.Zero;

            pData = Marshal.StringToHGlobalAnsi(String.Format("{0}\0", output));
            NativeMethods.MemCopy(data, pData, (uint)(output.Length + 1));

            Marshal.FreeHGlobal(pData);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.ASCII.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
