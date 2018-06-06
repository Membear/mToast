using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MircSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LOADINFO
    {
        public uint mVersion;
        public IntPtr mHwnd;
        public bool mKeep;
        public bool mUnicode;
        public uint mBeta;
    }

    class mReturn
    {
        public static int Halt = 0;
        public static int Continue = 1;
        public static int Command = 2;
        public static int Return = 3;
    }

    enum mTimeout : int
    {
        Manual,
        Timeout,
        Exit
    }

    public class mIRC
    {
        const int MIRC_MAP_SIZE = (8192 * sizeof(char));
        const string MIRC_MAP_NAME = "mIRC32";

        IntPtr hFile = IntPtr.Zero;
        IntPtr pView = IntPtr.Zero;

        const int cIndex = 32;

        public mIRC(IntPtr loadinfo)
        {
            LoadInfo = (LOADINFO)Marshal.PtrToStructure(loadinfo, typeof(LOADINFO));

            InitMapFile();
        }

        public LOADINFO LoadInfo;

        void InitMapFile()
        {
            try
            {
                // Create file mapping, and give access to all users with access to the namespace
                hFile = Win32.CreateFileMapping(Win32.INVALID_HANDLE_VALUE, IntPtr.Zero, Win32.PAGE_READWRITE, 0, MIRC_MAP_SIZE, MIRC_MAP_NAME);
                if (hFile == IntPtr.Zero) { throw new Exception("CreateFileMapping", new Win32Exception(Marshal.GetLastWin32Error())); }

                // Map file and write something to it
                pView = Win32.MapViewOfFile(hFile, Win32.SECTION_ALL_ACCESS, 0, 0, 0);
                if (pView == IntPtr.Zero) { throw new Exception("MapViewOfFile", new Win32Exception(Marshal.GetLastWin32Error())); }
            }
            finally
            {
            }
        }

        public bool Exec(string cmd)
        {
            IntPtr pData = IntPtr.Zero;

            pData = Marshal.StringToHGlobalAnsi(String.Format("{0}\0", cmd));
            Win32.MemCopy(pView, pData, (uint)(cmd.Length + 1));

            Marshal.FreeHGlobal(pData);

            return Win32.SendMessage(LoadInfo.mHwnd, Win32.WM_MCOMMAND, 0, cIndex) != 0;
        }

        public bool Eval(out string output, string input)
        {
            IntPtr pData = IntPtr.Zero;

            input = input + '\0';
            pData = Marshal.StringToHGlobalAnsi(input);
            Win32.MemCopy(pView, pData, (uint)input.Length);

            var ret = Win32.SendMessage(LoadInfo.mHwnd, Win32.WM_MEVALUATE, 0, cIndex);

            bool success = ret == 1;

            if (success)
            {
                output = Marshal.PtrToStringAnsi(pView);
                return true;
            }
            else
            {
                output = "";
            }

            return false;
        }

        public static string GetData(ref IntPtr data)
        {
            return Marshal.PtrToStringAnsi(data);
        }

        public static void SetData(ref IntPtr data, string output)
        {
            IntPtr pData = IntPtr.Zero;

            pData = Marshal.StringToHGlobalAnsi(String.Format("{0}\0", output));
            Win32.MemCopy(data, pData, (uint)(output.Length + 1));

            Marshal.FreeHGlobal(pData);
        }
    }
}
