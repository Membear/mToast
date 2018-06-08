using System;
using System.Runtime.InteropServices;

using DesktopToast;

namespace MircSharp.ToastNotifications
{
    /// <summary>
    /// Inherited class of notification activator (for Action Center of Windows 10)
    /// </summary>
    /// <remarks>The CLSID of this class must be unique for each application.</remarks>
    [Guid("3E463EE2-AEA2-402F-AA29-77914FAE279B"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    public class NotificationActivator : NotificationActivatorBase
    { }
}