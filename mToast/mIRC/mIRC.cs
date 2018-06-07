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

    static class mReturn
    {
        public const int Halt = 0;
        public const int Continue = 1;
        public const int Command = 2;
        public const int Return = 3;
    }

    static class UnloadReturn
    {
        public const int Keep = 0;
        public const int Allow = 1;
    }

    static class UnloadTimeout
    {
        public const int Manual = 0;
        public const int Timeout = 1;
        public const int Exit = 2;
    }

    public class mIRC : IDisposable
    {
        const int MIRC_MAP_SIZE = (8192 * sizeof(char));

        readonly IntPtr hFileMap = IntPtr.Zero;
        readonly IntPtr pView = IntPtr.Zero;
        readonly IntPtr pLoadInfo;

        readonly int cIndex;

        public LOADINFO LoadInfo { get; }

        public mIRC(IntPtr loadinfo)
        {
            pLoadInfo = loadinfo;
            LoadInfo = (LOADINFO)Marshal.PtrToStructure(loadinfo, typeof(LOADINFO));

            InitMapFile(ref cIndex, ref hFileMap, ref pView);
        }

        void InitMapFile(ref int cIndex, ref IntPtr hFileMap, ref IntPtr pView)
        {
            int error = 0;
            var r = new Random();
            
            cIndex = r.Next(1, int.MaxValue);

            do
            {
                cIndex = cIndex % (int.MaxValue - 1) + 1;
                string name = "mIRC" + cIndex;

                hFileMap = Win32.CreateFileMapping(Win32.INVALID_HANDLE_VALUE, IntPtr.Zero, Win32.PAGE_READWRITE, 0, MIRC_MAP_SIZE, name);

                if (hFileMap == IntPtr.Zero) return;

                error = Marshal.GetLastWin32Error();

                if (error == Win32.ERROR_ALREADY_EXISTS)
                {
                    Win32.CloseHandle(hFileMap);
                }
                else if (error > 0)
                {
                    return;
                }                

            } while (error == Win32.ERROR_ALREADY_EXISTS);

            pView = Win32.MapViewOfFile(hFileMap, Win32.SECTION_ALL_ACCESS, 0, 0, 0);
            if (pView == IntPtr.Zero)
            {
                Win32.CloseHandle(hFileMap);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                if (pView != IntPtr.Zero) Win32.UnmapViewOfFile(pView);
                if (hFileMap != IntPtr.Zero) Win32.CloseHandle(hFileMap);

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~mIRC()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
