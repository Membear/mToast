using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace MircSharp
{
    [Flags]
    enum FileMapProtection : uint
    {
        PageReadonly = 0x02,
        PageReadWrite = 0x04,
        PageWriteCopy = 0x08,
        PageExecuteRead = 0x20,
        PageExecuteReadWrite = 0x40,
        SectionCommit = 0x8000000,
        SectionImage = 0x1000000,
        SectionNoCache = 0x10000000,
        SectionReserve = 0x4000000,
    }

    internal static class NativeMethods
    {
        public const int ERROR_ALREADY_EXISTS = 183;        

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        
        public const int SECTION_QUERY                  = 0x0001;
        public const int SECTION_MAP_WRITE              = 0x0002;
        public const int SECTION_MAP_READ               = 0x0004;
        public const int SECTION_MAP_EXECUTE            = 0x0008;
        public const int SECTION_EXTEND_SIZE            = 0x0010;
        public const int SECTION_MAP_EXECUTE_EXPLICIT   = 0x0020; // not included in SECTION_ALL_ACCESS

        public const int SECTION_ALL_ACCESS =
            (SECTION_QUERY
            | SECTION_MAP_WRITE
            | SECTION_MAP_READ
            | SECTION_MAP_EXECUTE
            | SECTION_EXTEND_SIZE);

        public const int WM_USER = 0x0400;
        public const int WM_MCOMMAND = (WM_USER + 200);
        public const int WM_MEVALUATE = (WM_USER + 201);
                
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr lpFileMappingAttributes,
            FileMapProtection flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            [MarshalAs(UnmanagedType.LPStr)] string lpName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
        [In] IntPtr hFileMappingObject,
        [In] int dwDesiredAccess,
        [In] int dwFileOffsetHigh,
        [In] int dwFileOffsetLow,
        [In] int dwNumberOfBytesToMap
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemCopy(IntPtr dest, IntPtr src, uint count);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);        
    }
}