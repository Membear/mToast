using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;

using DesktopToast;
using MircSharp;
using System.IO;

namespace Toasty
{
    class mToast
    {
        const string AppId = "mIRC";

        static mIRC mInstance = null;

        public static mToast Instance { get; } = new mToast();

        static int ToastId { get; set; }

        static string LogoFilePath { get; set; }

        static string Line1 { get; set; } = "mToast Line1 Default";
        static string Line2 { get; set; } = "mToast Line2 Default";

        static string OnActivatedCallback { get; set; } = "mToast.OnActivated";
        static string OnCompleteCallback { get; set; } = "mToast.OnComplete";

        static mToast()
        {
        }

        public mToast()
        {
            NotificationActivatorBase.RegisterComType(typeof(NotificationActivator), OnActivated);
            NotificationHelper.RegisterComServer(typeof(NotificationActivator), Assembly.GetExecutingAssembly().Location);

            var image = Properties.Resources.mirclogo;
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var imagePath = Path.Combine(path, "mIRC.png");
            if (!File.Exists(imagePath))
            {
                File.WriteAllBytes(imagePath, image);
            }

            LogoFilePath = imagePath;
        }

        private void OnActivated(string arguments, Dictionary<string, string> data)
        {
            var result = "Activated";
            if ((arguments?.StartsWith("action=")).GetValueOrDefault())
            {
                result = arguments.Substring("action=".Length);
            }

            var serializer = new JavaScriptSerializer();

            const string Format = "//if ($isalias({0})) {{ var %args = {1}, %data = {2} | noop ${0}(%args,%data) }}";
            mInstance.Exec(string.Format(Format, OnActivatedCallback, arguments, serializer.Serialize(data)));
        }

        private async Task<string> ShowToastAsync()
        {
            var request = new ToastRequest
            {
                ToastTitle = Line1,
                ToastBody = Line2,
                ToastLogoFilePath = LogoFilePath,// Path.GetFullPath("Resources /toast128.png")),
                ShortcutFileName = AppId + ".lnk",
                ShortcutTargetFilePath = Process.GetCurrentProcess().MainModule.FileName,
                AppId = AppId,
                ActivatorId = typeof(NotificationActivator).GUID
            };

            var result = await ToastManager.ShowAsync(request);

            return result.ToString();
        }
        
        private async Task<string> ShowCustomToastAsync(string xml)
        {
            var request = new ToastRequest
            {
                ToastXml = xml,
                ShortcutFileName = AppId + ".lnk",
                ShortcutTargetFilePath = Process.GetCurrentProcess().MainModule.FileName,
                AppId = AppId,
                ActivatorId = typeof(NotificationActivator).GUID
            };

            var result = await ToastManager.ShowAsync(request);

            return result.ToString();
        }
        
        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static void LoadDll(IntPtr loadinfo)
        {
            mInstance = new mIRC(loadinfo);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int UnloadDll(int mTimeout)
        {
            return 1;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLine1(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Line1 = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLine2(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Line2 = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLogoPath(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            LogoFilePath = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetOnActivatedCallback(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            OnActivatedCallback = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetOnCompleteCallback(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            OnCompleteCallback = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int ShowToastAsync(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            int id = ++ToastId;

            Instance.ShowToastAsync().
                ContinueWith(result => mInstance.Exec(String.Format("//if ($isalias({0})) {{ {0} {1} {2} }}", OnCompleteCallback, id, result.Result)));

            mIRC.SetData(ref data, id.ToString());

            return mReturn.Return;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int ShowCustomToastAsync(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            string xml = mIRC.GetData(ref data);

            if (String.IsNullOrEmpty(xml))
            {
                return mReturn.Continue;
            }

            int id = ++ToastId;

            Instance.ShowCustomToastAsync(xml).
                ContinueWith(result => mInstance.Exec(String.Format("//if ($isalias({0})) {{ {0} {1} {2} }}", OnCompleteCallback, id, result.Result)));

            mIRC.SetData(ref data, id.ToString());

            return mReturn.Return;
        }
    }
}
