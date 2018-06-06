// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using DesktopNotifications;
using System;
using System.Runtime.InteropServices;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Toasty
{
    // The GUID CLSID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("23A5B06E-20BB-4E7E-A0AC-6982ED6A6041"), ComVisible(true)]
    public class MyNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
        {
            string s = arguments;

            string u = userInput.ToString();
            //Application.Current.Dispatcher.Invoke(delegate
            //{
            //if (arguments.Length == 0)
            //{
            //    OpenWindowIfNeeded();
            //    return;
            //}

            // Parse the query string (using NuGet package QueryString.NET)
            //QueryString args = QueryString.Parse(arguments);

            // See what action is being requested 
            //    switch (args["action"])
            //    {
            //        // Open the image
            //        case "viewImage":

            //            // The URL retrieved from the toast args
            //            string imageUrl = args["imageUrl"];

            //            // Make sure we have a window open and in foreground
            //            //OpenWindowIfNeeded();

            //            // And then show the image
            //            //(App.Current.Windows[0] as MainWindow).ShowImage(imageUrl);

            //            break;

            //        // Open the conversation
            //        case "viewConversation":

            //            // The conversation ID retrieved from the toast args
            //            int conversationId = int.Parse(args["conversationId"]);

            //            // Make sure we have a window open and in foreground
            //            OpenWindowIfNeeded();

            //            // And then show the conversation
            //            //(App.Current.Windows[0] as MainWindow).ShowConversation();

            //            break;

            //        // Background: Quick reply to the conversation
            //        case "reply":

            //            // Get the response the user typed
            //            string msg = userInput["tbReply"];

            //            // And send this message
            //            ShowToast("Sending message: " + msg);

            //            // If there's no windows open, exit the app
            //            //if (App.Current.Windows.Count == 0)
            //            //{
            //            //    Application.Current.Shutdown();
            //            //}

            //            break;

            //        // Background: Send a like
            //        case "like":

            //            ShowToast("Sending like");

            //            // If there's no windows open, exit the app
            //            //if (App.Current.Windows.Count == 0)
            //            //{
            //            //    Application.Current.Shutdown();
            //            //}

            //            break;

            //        default:

            //            OpenWindowIfNeeded();

            //            break;
            //    }
            ////});
        }

        private void OpenWindowIfNeeded()
        {
            //// Make sure we have a window open (in case user clicked toast while app closed)
            //if (App.Current.Windows.Count == 0)
            //{
            //    new MainWindow().Show();
            //}

            //// Activate the window, bringing it to focus
            //App.Current.Windows[0].Activate();

            //// And make sure to maximize the window too, in case it was currently minimized
            //App.Current.Windows[0].WindowState = WindowState.Normal;
        }

        public static void ShowToast2(string msg)
        {
            string content = "<toast launch=\"action=viewConversation&amp;conversationId=5\"><visual><binding template=\"ToastGeneric\"><text>Andrew sent you a picture</text><text>Check this out, The Enchantments!</text><image src=\"C:\\Users\\Daniel\\AppData\\Local\\Temp\\WindowsNotifications.DesktopToasts.Images\\5\\2166872428\" /><image src=\"C:\\Users\\Daniel\\AppData\\Local\\Temp\\WindowsNotifications.DesktopToasts.Images\\5\\2591940874\" placement=\"appLogoOverride\" hint-crop=\"circle\" /></binding></visual><actions><input id=\"tbReply\" type=\"text\" placeHolderContent=\"Type a response\" /><action content=\"Reply\" arguments=\"action=reply&amp;conversationId=5\" /><action content=\"Like\" arguments=\"action=like&amp;conversationId=5\" /><action content=\"View\" arguments=\"action=viewImage&amp;imageUrl=https%3A%2F%2Fpicsum.photos%2F364%2F202%3Fimage%3D883\" /></actions></toast>";

            //string content = "<?xml version=\"1.0\" encoding=\"utf-8\"?><toast launch=\"action=ok\"><visual><binding template=\"ToastGeneric\"><text>Sending like</text></binding></visual></toast>";

            var doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(content);

            // And create the toast notification
            var toast = new ToastNotification(doc);

            // And then show it
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        public static void ShowToast(string title, string line)
        {
            //string content = "<? xml version =\"1.0\" encoding=\"utf-8\"?><toast launch=\"action=viewConversation&amp;conversationId=5\"><visual><binding template=\"ToastGeneric\"><text>Andrew sent you a picture</text><text>Check this out, The Enchantments!</text><image src=\"C:\\Users\\Daniel\\AppData\\Local\\Temp\\WindowsNotifications.DesktopToasts.Images\\5\\2166872428\" /><image src=\"C:\\Users\\Daniel\\AppData\\Local\\Temp\\WindowsNotifications.DesktopToasts.Images\\5\\2591940874\" placement=\"appLogoOverride\" hint-crop=\"circle\" /></binding></visual><actions><input id=\"tbReply\" type=\"text\" placeHolderContent=\"Type a response\" /><action content=\"Reply\" arguments=\"action=reply&amp;conversationId=5\" /><action content=\"Like\" arguments=\"action=like&amp;conversationId=5\" /><action content=\"View\" arguments=\"action=viewImage&amp;imageUrl=https%3A%2F%2Fpicsum.photos%2F364%2F202%3Fimage%3D883\" /></actions></toast>";

            string content = String.Format("<toast launch=\"action=ok\"><visual><binding template=\"ToastGeneric\"><image placement=\"appLogoOverride\" src=\"C:\\Users\\Daniel\\AppData\\Roaming\\mIRC\\mIRC.png\" /><text hint-maxLines=\"1\">{0}</text><text>{1}</text></binding></visual></toast>", title, line);

            var doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(content);

            // And create the toast notification
            var toast = new ToastNotification(doc);

            // And then show it
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }
    }
}