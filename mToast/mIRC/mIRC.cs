using System;
using System.Runtime.InteropServices;

namespace MircSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LOADINFO
    {
        public readonly uint mVersion;
        public readonly IntPtr mHwnd;
        public readonly bool mKeep;
        public readonly bool mUnicode;
        public readonly uint mBeta;
    }

    static class ReturnType
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
        const int MIRC_LINE_LENGTH = 4150;
        const int MIRC_MAP_SIZE = (MIRC_LINE_LENGTH * sizeof(char));

        readonly IntPtr hFileMap = IntPtr.Zero;
        readonly IntPtr pView = IntPtr.Zero;
        readonly IntPtr pLoadInfo;

        readonly IntPtr cIndex;

        public LOADINFO LoadInfo { get; }

        public mIRC(IntPtr loadinfo)
        {
            pLoadInfo = loadinfo;
            LoadInfo = (LOADINFO)Marshal.PtrToStructure(loadinfo, typeof(LOADINFO));

            InitMapFile(ref cIndex, ref hFileMap, ref pView);
        }

        void InitMapFile(ref IntPtr cIndex, ref IntPtr hFileMap, ref IntPtr pView)
        {
            int error = 0;
            var r = new Random();
            
            cIndex = (IntPtr)r.Next(1, int.MaxValue);

            do
            {
                cIndex = (IntPtr)((int)cIndex % (int.MaxValue - 1) + 1);
                string name = "mIRC" + cIndex;

                hFileMap = NativeMethods.CreateFileMapping(NativeMethods.INVALID_HANDLE_VALUE, IntPtr.Zero, FileMapProtection.PageReadWrite, 0, MIRC_MAP_SIZE, name);

                if (hFileMap == IntPtr.Zero) return;

                error = Marshal.GetLastWin32Error();

                if (error == NativeMethods.ERROR_ALREADY_EXISTS)
                {
                    NativeMethods.CloseHandle(hFileMap);
                }
                else if (error > 0)
                {
                    return;
                }                

            } while (error == NativeMethods.ERROR_ALREADY_EXISTS);

            pView = NativeMethods.MapViewOfFile(hFileMap, NativeMethods.SECTION_ALL_ACCESS, 0, 0, 0);
            if (pView == IntPtr.Zero)
            {
                NativeMethods.CloseHandle(hFileMap);
            }
        }

        /// <summary>
        /// Sends a command to mIRC by use of SendMessage
        /// </summary>
        /// <exception cref="RPC_E_CANTCALLOUT_ININPUTSYNCCALL">
        /// May occur if the dll is reentered as a result of this call.
        /// Avoid by calling /timer or starting a new thread.
        /// </exception>
        /// <param name="cmd">Command to send</param>
        /// <returns>Success</returns>
        public bool Exec(string cmd)
        {
            IntPtr pData = IntPtr.Zero;

            pData = Marshal.StringToHGlobalAnsi(String.Format("{0}\0", cmd.Truncate(MIRC_LINE_LENGTH - 1)));
            NativeMethods.MemCopy(pView, pData, (uint)(cmd.Length + 1));

            Marshal.FreeHGlobal(pData);

            return NativeMethods.SendMessage(LoadInfo.mHwnd, NativeMethods.WM_MCOMMAND, IntPtr.Zero, cIndex) != IntPtr.Zero;
        }

        public bool Eval(out string output, string input)
        {
            IntPtr pData = IntPtr.Zero;

            input = input + '\0';
            pData = Marshal.StringToHGlobalAnsi(input);
            NativeMethods.MemCopy(pView, pData, (uint)input.Length);

            var ret = NativeMethods.SendMessage(LoadInfo.mHwnd, NativeMethods.WM_MEVALUATE, IntPtr.Zero, cIndex);

            bool success = ret != IntPtr.Zero;

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

                if (pView != IntPtr.Zero) NativeMethods.UnmapViewOfFile(pView);
                if (hFileMap != IntPtr.Zero) NativeMethods.CloseHandle(hFileMap);

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
