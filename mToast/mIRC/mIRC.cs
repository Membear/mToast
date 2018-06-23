using System;
using System.Runtime.InteropServices;

namespace MircSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LOADINFO
    {
        public readonly uint mVersion;
        public readonly IntPtr mHwnd;
        public bool mKeep;
        public bool mUnicode;
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

    static class cMethod
    {
        public const int EditBox = 1;
        public const int PlainText = 2;
        public const int Flood = 4;
        public const int Unicode = 8;

        public static readonly IntPtr pEditBox = new IntPtr(1);
        public static readonly IntPtr pPlainText = new IntPtr(2);
        public static readonly IntPtr pFlood = new IntPtr(4);
        public static readonly IntPtr pUnicode = new IntPtr(8);
    }

    public class mIRC : IDisposable
    {
        const int MIRC_LINE_LENGTH = 4150;
        const int MIRC_MAP_SIZE = (MIRC_LINE_LENGTH * sizeof(char));

        readonly IntPtr hFileMap = IntPtr.Zero;
        readonly IntPtr pView = IntPtr.Zero;
        readonly IntPtr cIndex = IntPtr.Zero;

        public LOADINFO LoadInfo { get; private set; }
        
        public mIRC(ref LOADINFO loadinfo)
            :base()
        {
            Load(ref loadinfo);
        }

        public mIRC()
        {
            InitMapFile(ref cIndex, ref hFileMap, ref pView);
        }

        public void Load(ref LOADINFO loadinfo)
        {
            loadinfo.mUnicode = true;
            LoadInfo = loadinfo;
        }
        
        void InitMapFile(ref IntPtr cIndex, ref IntPtr hFileMap, ref IntPtr pView)
        {
            int error = 0;
            var r = new Random();
            
            int index = r.Next(1, int.MaxValue);

            do
            {
                index = index % (int.MaxValue - 1) + 1;
                string name = $"mIRC{index}";

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
            else
            {
                cIndex = (IntPtr)index;
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
        public bool Exec(in string input)
        {
            string trunc = input.Truncate(MIRC_LINE_LENGTH);

            IntPtr pData = Marshal.StringToHGlobalUni(trunc);
            NativeMethods.MemCopy(pView, pData, (uint)(trunc.Length + 1) * sizeof(char));

            Marshal.FreeHGlobal(pData);

            return NativeMethods.SendMessage(LoadInfo.mHwnd, NativeMethods.WM_MCOMMAND, cMethod.pUnicode, cIndex) != IntPtr.Zero;
        }

        public bool Eval(out string output, in string input)
        {
            string trunc =  input.Truncate(MIRC_LINE_LENGTH);
            
            IntPtr pData = Marshal.StringToHGlobalUni(trunc);
            NativeMethods.MemCopy(pView, pData, (uint)(trunc.Length + 1) * sizeof(char));
            
            if (IntPtr.Zero != NativeMethods.SendMessage(LoadInfo.mHwnd, NativeMethods.WM_MEVALUATE, cMethod.pUnicode, cIndex))
            {
                output = Marshal.PtrToStringUni(pView);
                return true;
            }
            else
            {
                output = null;
                return false;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (pView != IntPtr.Zero) NativeMethods.UnmapViewOfFile(pView);
                if (hFileMap != IntPtr.Zero) NativeMethods.CloseHandle(hFileMap);

                disposedValue = true;
            }
        }
        
        ~mIRC()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
