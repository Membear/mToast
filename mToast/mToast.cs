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
        #region Constants
        const string AppId = "mIRC";

        enum RequestType
        {
            Xml,
            Json
        }
        #endregion

        #region Members
        public static mToast Instance { get; } = new mToast();

        mIRC mInstance { get; set; }
        #endregion

        #region Properties
        bool _revertTag;
        int _tagCounter;
        string _tag;
        string NextTag
        {
            get
            {
                if (_revertTag)
                {
                    _revertTag = false;

                    var _temp = _tag;
                    _tag = _tagCounter.ToString();

                    return _temp;
                }

                return (++_tagCounter).ToString();
            }
            set
            {
                _revertTag = true;
                _tag = value;
            }
        }

        bool _revertGroup;
        string _group;
        string NextGroup
        {
            get
            {
                if (_revertGroup)
                {
                    _revertGroup = false;
                    return _group;
                }

                return "mToast";
            }
            set
            {
                _revertGroup = true;
                _group = value;
            }
        }
        
        string OnActivatedCallback { get; set; } = "mToast.OnActivated";
        string OnCompleteCallback { get; set; } = "mToast.OnComplete";
        #endregion

        #region Constructors / Initialization
        static mToast()
        {
        }

        public mToast()
        {
            NotificationActivatorBase.RegisterComType(typeof(NotificationActivator), OnActivated);
            NotificationHelper.RegisterComServer(typeof(NotificationActivator), Assembly.GetExecutingAssembly().Location);
        }

        private void CreateShortcut()
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
        }

        private void CopyIcon()
        {
            var imagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mIRC.png");

            if (!File.Exists(imagePath))
            {
                File.WriteAllBytes(imagePath, Properties.Resources.mirclogo);
            }
        }
        #endregion

        #region Notification Creation/Handling
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
        
        private async Task<(ToastResult, ToastRequest)> ShowToastAsync(ToastRequest request)
        {
            (ToastResult res, ToastRequest req) tuple;

            tuple.req = request;
            tuple.res = await ToastManager.ShowAsync(request);

            return tuple;
        }

        private static int ShowToast(RequestType type, ref IntPtr data)
        {
            string input = Utilities.GetData(ref data);

            if (String.IsNullOrWhiteSpace(input))
            {
                return ReturnType.Continue;
            }

            ToastRequest request;

            switch (type)
            {
                case RequestType.Xml:
                    request = new ToastRequest
                    {
                        ToastXml = input,
                        AppId = AppId,
                        ActivatorId = typeof(NotificationActivator).GUID
                    };
                    break;
                case RequestType.Json:
                    request = new ToastRequest(input)
                    {
                        AppId = AppId,
                        ActivatorId = typeof(NotificationActivator).GUID
                    };
                    break;
                default:
                    return ReturnType.Continue;
            }

            if (string.IsNullOrWhiteSpace(request.Group)) request.Group = Instance.NextGroup;
            if (string.IsNullOrWhiteSpace(request.Tag)) request.Tag = Instance.NextTag;

            const string Format = "/.timer 1 0 if ($isalias({0})) {{ noop ${0}($unsafe({1}).undo,{2}) }}";
            Instance.ShowToastAsync(request).
                ContinueWith(result => {
                    (var toastResult, var toastRequest) = result.Result;
                    Instance.mInstance.Exec(String.Format(Format,
                        Instance.OnCompleteCallback,
                        Utilities.Base64Encode(toastRequest.Tag),
                        toastResult));
                });

            Utilities.SetData(ref data, request.Tag);

            return ReturnType.Return;
        }
        #endregion

        #region mIRC Exports
            #region DLL Loading
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
            #endregion

            #region Auxiliary
            [DllExport(CallingConvention = CallingConvention.StdCall)]
            public static int Initialize(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
            {
                Instance.CreateShortcut();
                Instance.CopyIcon();

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
            public static int SetNextTag(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
            {
                var tag = Utilities.GetData(ref data);
                if (string.IsNullOrWhiteSpace(tag))
                {
                    _ = Instance.NextTag;
                }
                else
                {
                    Instance.NextTag = tag;
                }

                return ReturnType.Continue;
            }

            [DllExport(CallingConvention = CallingConvention.StdCall)]
            public static int SetNextGroup(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
            {
                var group = Utilities.GetData(ref data);
                if (string.IsNullOrWhiteSpace(group))
                {
                    _ = Instance.NextGroup;
                }
                else
                {
                    Instance.NextGroup = Utilities.GetData(ref data);
                }

                return ReturnType.Continue;
            }
            #endregion

            #region Toast Creation
            [DllExport(CallingConvention = CallingConvention.StdCall)]
            public static int ShowToastXml(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
            {
                try
                {
                    return ShowToast(RequestType.Xml, ref data);
                }
                catch (Exception e)
                {
                    Instance.mInstance.Exec(String.Format("/.timer 1 0 echo -sag mToast error: {0} {1}", e.Message, e.InnerException));
                    return ReturnType.Continue;
                }            
            }

            [DllExport(CallingConvention = CallingConvention.StdCall)]
            public static int ShowToastJson(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
            {
                try
                {
                    return ShowToast(RequestType.Json, ref data);
                }
                catch (Exception e)
                {
                    Instance.mInstance.Exec(String.Format("/.timer 1 0 echo -sag mToast error: {0} {1}", e.Message, e.InnerException));
                    return ReturnType.Continue;
                }
            }
            #endregion

            #region Toast History
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
                    ToastNotificationManager.History.Remove(Utilities.GetData(ref data), Instance.NextGroup, AppId);
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

            #endregion
        #endregion
    }
}
