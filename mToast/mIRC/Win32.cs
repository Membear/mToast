using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
 
namespace MircSharp
{
    class Win32
    {
        public const int SECURITY_MAX_SID_SIZE = 68;
        public const int SDDL_REVISION_1 = 1;
        public const uint INVALID_HANDLE_VALUE = 0xffffffff;
        public const int PAGE_READWRITE = 0x04;
        public const int FILE_MAP_WRITE = 0X02;

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

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateBoundaryDescriptor
        (
        [In] string Name,
        [In] int Flags
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CreateWellKnownSid
        (
        [In] WellKnownSidType WellKnownSidType,
        [In] [Optional] IntPtr DomainSid,
        [In] IntPtr pSid,
        [In][Out]ref int cbSid
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AddSIDToBoundaryDescriptor
        (
        [In][Out] ref IntPtr BoundaryDescriptor,
        [In] IntPtr RequiredSid
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor
        (
        [In] string StringSecurityDescriptor,
        [In] int StringSDRevision,
        [Out] out IntPtr SecurityDescriptor,
        [Out] IntPtr SecurityDescriptorSize
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree([In] IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreatePrivateNamespace(
        [In][Optional] ref SECURITY_ATTRIBUTES lpPrivateNamespaceAttributes,
        [In] IntPtr lpBoundaryDescriptor,
        [In] string lpAliasPrefix
        );

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr CreateFileMapping(
        [In] uint hFile,
        //[In][Optional] ref SECURITY_ATTRIBUTES lpAttributes,
        [In] IntPtr lpFileMappingAttributes,
        [In] int flProtect,
        [In] int dwMaximumSizeHigh,
        [In] int dwMaximumSizeLow,
        [In][Optional] string lpName
        );

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //public static extern IntPtr CreateFileMapping(
        //    IntPtr hFile,
        //    IntPtr lpFileMappingAttributes,
        //    FileMapProtection flProtect,
        //    uint dwMaximumSizeHigh,
        //    uint dwMaximumSizeLow,
        //    [MarshalAs(UnmanagedType.LPStr)] string lpName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
        [In] IntPtr hFileMappingObject,
        [In] int dwDesiredAccess,
        [In] int dwFileOffsetHigh,
        [In] int dwFileOffsetLow,
        [In] int dwNumberOfBytesToMap
        );

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemCopy(IntPtr dest, IntPtr src, uint count);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    }
}