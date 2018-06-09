using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;

using DesktopToast;
using Windows.UI.Notifications;

namespace MircSharp.ToastNotifications
{
    class mToast
    {
        const string AppId = "mIRC";

        public static mToast Instance { get; } = new mToast();

        mIRC mInstance { get; set; }

        int ToastId { get; set; }

        string Group { get; set; } = "mToast";
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
            string dataString;

            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(Dictionary<string,string>), new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                });
                serializer.WriteObject(stream, data);

                dataString = Encoding.ASCII.GetString(stream.ToArray());
            }

            const string Format = "/.timer 1 0 if ($isalias({0})) {{ noop ${0}($unsafe({1}).undo,$unsafe({2}).undo) }}";
            mInstance.Exec(string.Format(Format,
                OnActivatedCallback,
                string.IsNullOrEmpty(arguments) ? "$null" : Utilities.Base64Encode(arguments),
                data.Count == 0 ? "$null" : Utilities.Base64Encode(dataString)));
        }

        private async Task<string> ShowToastAsync()
        {
            var request = new ToastRequest
            {
                Group = Group,
                Tag = ToastId.ToString(),
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
                Group = Group,
                Tag = ToastId.ToString(),
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

            return ReturnType.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLine1(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.Line1 = Utilities.GetData(ref data);

            return ReturnType.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLine2(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.Line2 = Utilities.GetData(ref data);

            return ReturnType.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetLogoPath(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.LogoFilePath = Utilities.GetData(ref data);

            return ReturnType.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetOnActivatedCallback(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.OnActivatedCallback = Utilities.GetData(ref data);

            return ReturnType.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetOnCompleteCallback(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.OnCompleteCallback = Utilities.GetData(ref data);

            return ReturnType.Continue;
        }
        
        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int SetGroup(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.Group = Utilities.GetData(ref data);

            return ReturnType.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int ShowToastAsync(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            int id = ++Instance.ToastId;

            const string Format = "/.timer 1 0 if ($isalias({0})) {{ {0} {1} {2} }}";
            Instance.ShowToastAsync().
                ContinueWith(result => Instance.mInstance.Exec(String.Format(Format, Instance.OnCompleteCallback, id, result.Result)));

            Utilities.SetData(ref data, id.ToString());

            return ReturnType.Return;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int ShowCustomToastAsync(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            string xml = Utilities.GetData(ref data);

            if (String.IsNullOrEmpty(xml))
            {
                return ReturnType.Continue;
            }

            int id = ++Instance.ToastId;

            const string Format = "/.timer 1 0 if ($isalias({0})) {{ {0} {1} {2} }}";
            Instance.ShowToastAsync(xml).
                ContinueWith(result => Instance.mInstance.Exec(String.Format(Format, Instance.OnCompleteCallback, id, result.Result)));

            Utilities.SetData(ref data, id.ToString());

            return ReturnType.Return;
        }        

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int Clear(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            try
            {
                ToastNotificationManager.History.Clear(AppId);
            }
            catch (Exception e)
            {
                Utilities.SetData(ref data,String.Format("mToast error: {0} {1}", e.Message, e.ToString()));
            }

            return ReturnType.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int Remove(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            try
            {
                ToastNotificationManager.History.Remove(Utilities.GetData(ref data), Instance.Group, AppId);
            }
            catch (Exception e)
            {
                Utilities.SetData(ref data, String.Format("mToast error: {0} {1}", e.Message, e.ToString()));
            }

            return ReturnType.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int RemoveGroup(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            try
            {
                ToastNotificationManager.History.RemoveGroup(Utilities.GetData(ref data), AppId);
            }
            catch (Exception e)
            {
                Utilities.SetData(ref data, String.Format("mToast error: {0} {1}", e.Message, e.ToString()));
            }

            return ReturnType.Continue;
        }
    }
}
