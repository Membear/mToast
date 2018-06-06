using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;

using DesktopToast;
using MircSharp;

namespace Toasty
{
    class mToast
    {
        const string AppId = "mIRC";

        static mIRC mInstance = null;

        public static mToast Instance { get; } = new mToast();

        static string Line1 = "";
        static string Line2 = "";

        public string ToastResult { get; set; }

        private async Task<string> ShowToastAsync()
        {
            var request = new ToastRequest
            {
                ToastTitle = "DesktopToast WPF Sample",
                ToastBody = "This is a toast test.",
                ToastLogoFilePath = string.Format("file:///{0}", @"C:\Users\Daniel\AppData\Roaming\mIRC\toastImageAndText.png"),// Path.GetFullPath("Resources /toast128.png")),
                ShortcutFileName = AppId + ".lnk",
                ShortcutTargetFilePath = Process.GetCurrentProcess().MainModule.FileName,
                AppId = AppId,
                ActivatorId = typeof(NotificationActivator).GUID // For Action Center of Windows 10
            };

            var result = await ToastManager.ShowAsync(request);

            return result.ToString();
        }

        private void OnActivated(string arguments, Dictionary<string, string> data)
        {
            var result = "Activated";
            if ((arguments?.StartsWith("action=")).GetValueOrDefault())
            {
                result = arguments.Substring("action=".Length);

                //if ((data?.ContainsKey(MessageId)).GetValueOrDefault())
                //    Dispatcher.Invoke(() => Message = data[MessageId]);
            }
            //Dispatcher.Invoke(() => ActivationResult = result);
        }

        static mToast()
        {
            //DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("mIRC");
            //DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();

        }

        public mToast()
        {
            NotificationActivatorBase.RegisterComType(typeof(NotificationActivator), OnActivated);
            NotificationHelper.RegisterComServer(typeof(NotificationActivator), Assembly.GetExecutingAssembly().Location);
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
        public static int ShowToastAsync(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            Instance.ShowToastAsync().
                ContinueWith(result => mInstance.Exec(String.Format("/echo -sag Result: {0}", result.Result)));
            //.Wait();

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int ShowToast(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            string input = mIRC.GetData(ref data);

            //MyNotificationActivator.ShowToast(Line1, Line2);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int ShowToast2(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            string input = mIRC.GetData(ref data);

            //MyNotificationActivator.ShowToast2(input);

            return mReturn.Continue;
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static int Test(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        {
            string inStr = mIRC.GetData(ref data);
            string outStr = String.Format("Returning from C#, data = {0}", inStr);

            mInstance.Eval(out string eval, "$mircdir");
            mInstance.Exec(String.Format("/echo -sag SendMessage() Exec() && Eval() test, $mircdir = {0}", eval));

            mIRC.SetData(ref data, outStr);

            return mReturn.Return;
        }


        //[DllExport(CallingConvention = CallingConvention.StdCall)]
        //public static int CreateShortcut(IntPtr mWnd, IntPtr aWnd, IntPtr data, IntPtr parms, bool show, bool nopause)
        //{
        //    try
        //    {
        //        using (ShellLink shortcut = new ShellLink())
        //        {
        //            shortcut.TargetPath = Process.GetCurrentProcess().MainModule.FileName;
        //            shortcut.Arguments = "";
        //            shortcut.AppUserModelID = "mIRC";

        //            shortcut.Save(@"c:\users\daniel\desktop\mIRC.lnk");                    
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    return mReturn.Continue;
        //}

        //void ReadShortCut()
        //{
        //    if (!File.Exists(textBox_ShortcutFile.Text))
        //    {
        //        MessageBox.Show("Such shortcut file does not exist.", "",
        //                        MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return;
        //    }

        //    // Read shortcut file.
        //    try
        //    {
        //        using (ShellLink shortcut = new ShellLink(textBox_ShortcutFile.Text))
        //        {
        //            textBox_TargetPath.Text = shortcut.TargetPath;
        //            textBox_Arguments.Text = shortcut.Arguments;
        //            textBox_AppUserModelID.Text = shortcut.AppUserModelID;

        //            MessageBox.Show("Red shortcut file.", "",
        //                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Could not read shortcut file. " + ex.Message, "",
        //                        MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
    }

}
