using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;

using DesktopToast;
using MircSharp;

namespace Toasty
{
    class mToast
    {
        const string AppId = "mIRC";

        public static mToast Instance { get; } = new mToast();

        mIRC mInstance { get; set; }

        int ToastId { get; set; }

        string LogoFilePath { get; set; }

        string Line1 { get; set; } = "mIRC Toast Notifications";
        string Line2 { get; set; } = "by Membear";

        string OnActivatedCallback { get; set; } = "mToast.OnActivated";
        string OnCompleteCallback { get; set; } = "mToast.OnComplete";

        static mToast()
        {
        }

        public mToast()
        {
            NotificationActivatorBase.RegisterComType(typeof(NotificationActivator), OnActivated);
            NotificationHelper.RegisterComServer(typeof(NotificationActivator), Assembly.GetExecutingAssembly().Location);
            
            var image = Properties.Resources.mirclogo;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var imagePath = Path.Combine(path, "mIRC.png");

            if (!File.Exists(imagePath))
            {
                File.WriteAllBytes(imagePath, image);
            }

            LogoFilePath = imagePath;
        }

        private void OnActivated(string arguments, Dictionary<string, string> data)
        {
            var serializer = new JavaScriptSerializer();
            
            const string Format = "//if ($isalias({0})) {{ var %args = $unsafe({1}).undo, %data = $unsafe({2}).undo | noop ${0}(%args,%data) }}";
            mInstance.Exec(string.Format(Format,
                OnActivatedCallback,
                string.IsNullOrEmpty(arguments) ? "$null" : Utilities.Base64Encode(arguments),
                data.Count == 0 ? "$null" : Utilities.Base64Encode(serializer.Serialize(data))));
        }

        private async Task<string> ShowToastAsync()
        {
            var request = new ToastRequest
            {
                ToastTitle = Line1,
                ToastBody = Line2,
                ToastLogoFilePath = LogoFilePath,
                AppId = AppId,
                ActivatorId = typeof(NotificationActivator).GUID
            };

            var result = await ToastManager.ShowAsync(request);

            return result.ToString();
        }
        
        private async Task<string> ShowToastAsync(string xml)
        {
            var request = new ToastRequest
            {
                ToastXml = xml,
                AppId = AppId,
                ActivatorId = typeof(NotificationActivator).GUID
            };

            var result = await ToastManager.ShowAsync(request);

            return result.ToString();
        }
        
        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static void LoadDll(IntPtr loadinfo)
        {
            Instance.mInstance = new mIRC(loadinfo);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int UnloadDll(int mTimeout)
        {
            if (mTimeout == UnloadTimeout.Timeout)
            {
                return UnloadReturn.Keep;
            }

            Instance.mInstance.Dispose();

            return UnloadReturn.Allow;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int Initialize(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            var req = new ToastRequest()
            {
                ShortcutFileName = AppId + ".lnk",
                ShortcutTargetFilePath = Process.GetCurrentProcess().MainModule.FileName,
                AppId = AppId,
                ActivatorId = typeof(NotificationActivator).GUID,
                WaitingDuration = TimeSpan.Zero,
            };
            _ = ToastManager.CheckInstallShortcut(req);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLine1(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.Line1 = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLine2(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.Line2 = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLogoPath(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.LogoFilePath = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetOnActivatedCallback(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.OnActivatedCallback = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetOnCompleteCallback(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.OnCompleteCallback = mIRC.GetData(ref data);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int ShowToastAsync(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            int id = ++Instance.ToastId;

            Instance.ShowToastAsync().
                ContinueWith(result => Instance.mInstance.Exec(String.Format("//if ($isalias({0})) {{ {0} {1} {2} }}", Instance.OnCompleteCallback, id, result.Result)));

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

            int id = ++Instance.ToastId;

            Instance.ShowToastAsync(xml).
                ContinueWith(result => Instance.mInstance.Exec(String.Format("//if ($isalias({0})) {{ {0} {1} {2} }}", Instance.OnCompleteCallback, id, result.Result)));

            mIRC.SetData(ref data, id.ToString());

            return mReturn.Return;
        }        
    }
}
