using System;
using System.Runtime.InteropServices;

namespace MircSharp
{
    /// <summary>
    /// Stuct passed from mIRC in LoadDll
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LOADINFO
    {
        /// <summary>
        /// Contains the mIRC version number in the low and high word.
        /// </summary>
        public readonly uint mVersion;

        /// <summary>
        /// Contains the window handle to the main mIRC window.
        /// </summary>
        public readonly IntPtr mHwnd;

        /// <summary>
        /// Indicates that mIRC will keep the DLL loaded after the call.
        /// </summary>
        /// <remarks>
        /// Is set to TRUE by default.
        /// You can set mKeep to FALSE to make mIRC unload the DLL after the call.
        /// </remarks>
        public bool mKeep;

        /// <summary>
        /// Indicates that text is in unicode as opposed to ansi.
        /// </summary>
        public bool mUnicode;

        /// <summary>
        /// Contains the mIRC beta version number, for public betas.
        /// </summary>
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

    /// <summary>
    /// Provides support for SendMessage() communication with mIRC
    /// </summary>
    public class mIRC : IDisposable
    {
        const int MIRC_LINE_LENGTH = 4150;
        const int MIRC_MAP_SIZE = (MIRC_LINE_LENGTH * sizeof(char));

        readonly IntPtr hFileMap = IntPtr.Zero;
        readonly IntPtr pView = IntPtr.Zero;
        readonly IntPtr cIndex = IntPtr.Zero;

        public LOADINFO LoadInfo { get; private set; }
        
        /// <summary>
        /// Calls <see cref="Load(ref LOADINFO)"/> method
        /// </summary>
        /// <param name="loadinfo"></param>
        public mIRC(ref LOADINFO loadinfo)
            :base()
        {
            Load(ref loadinfo);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public mIRC()
        {
            InitMapFile(ref cIndex, ref hFileMap, ref pView);
        }

        /// <summary>
        /// Stores <see cref="LOADINFO"/> object
        /// Required to use <see cref="Exec(in string)"/> and <see cref="Eval(out string, in string)"/>
        /// </summary>
        /// <param name="loadinfo"><see cref="LOADINFO"/> struct from LoadDll method</param>
        public void Load(ref LOADINFO loadinfo)
        {
            loadinfo.mUnicode = true;
            LoadInfo = loadinfo;
        }
        
        void InitMapFile(ref IntPtr cIndex, ref IntPtr hFileMap, ref IntPtr pView)
        {
            var r = new Random();

            int index = r.Next(1, int.MaxValue);
            int error;

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
        /// <exception>
        /// RPC_E_CANTCALLOUT_ININPUTSYNCCALL
        /// May occur if the dll is reentered as a result of this call.
        /// Avoid by calling /timer or starting a new thread
        /// </exception>
        /// <param name="input">Command to send</param>
        /// <returns>Success</returns>
        public bool Exec(in string input)
        {
            IntPtr pData;

            if (input.Length > MIRC_LINE_LENGTH)
            {
                pData = Marshal.StringToHGlobalUni(input.Substring(0, MIRC_LINE_LENGTH));
            }
            else
            {
                pData = Marshal.StringToHGlobalUni(input);
            }

            NativeMethods.MemCopy(pView, pData, (uint)(Math.Min(input.Length, MIRC_LINE_LENGTH) + 1) * sizeof(char));

            Marshal.FreeHGlobal(pData);

            return NativeMethods.SendMessage(LoadInfo.mHwnd, NativeMethods.WM_MCOMMAND, cMethod.pUnicode, cIndex) != IntPtr.Zero;
        }

        /// <summary>
        /// Passes text to mIRC for evaluation
        /// </summary>
        /// <param name="output">Evaluated string</param>
        /// <param name="input">String to evaluate</param>
        /// <returns>Success</returns>
        public bool Eval(out string output, in string input)
        {
            IntPtr pData;

            if (input.Length > MIRC_LINE_LENGTH)
            {                
                pData = Marshal.StringToHGlobalUni(input.Substring(0, MIRC_LINE_LENGTH));
            }
            else
            {
                pData = Marshal.StringToHGlobalUni(input);
            }
            
            NativeMethods.MemCopy(pView, pData, (uint)(Math.Min(input.Length, MIRC_LINE_LENGTH) + 1) * sizeof(char));

            Marshal.FreeHGlobal(pData);

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

        /// <summary>
        /// Dispose pattern
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (pView != IntPtr.Zero) NativeMethods.UnmapViewOfFile(pView);
                if (hFileMap != IntPtr.Zero) NativeMethods.CloseHandle(hFileMap);

                disposedValue = true;
            }
        }
        
        /// <summary>
        /// Dispose pattern
        /// </summary>
        ~mIRC()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// Dispose pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
