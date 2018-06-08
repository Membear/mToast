using System;
using System.Runtime.InteropServices;

namespace MircSharp
{
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
