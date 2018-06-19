using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DesktopToast
{
	/// <summary>
	/// Toast request container
	/// </summary>
	[DataContract]
	public class ToastRequest
	{
		#region Public Property

		/// <summary>
		/// Toast title (optional)
		/// </summary>
		[DataMember]
		public string Title { get; set; }

		/// <summary>
		/// Toast body (required for toast)
		/// </summary>
		[DataMember]
		public string Body
		{
			get { return BodyList?[0]; }
			set { BodyList = new string[] { value }; }
		}

		/// <summary>
		/// Toast body list (optional)
		/// </summary>
		/// <remarks>If specified, toast body will be substituted by this list.</remarks>
		[DataMember]
		public IList<string> BodyList
		{
			get { return _bodyList; }
			set { _bodyList = value?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList(); }
		}
		private IList<string> _bodyList;

		/// <summary>
		/// Logo image file path of toast (optional)
		/// </summary>
		/// <remarks>
		/// This file path must be in the following form:
		/// "file:///" + full file path
		/// </remarks>
		[DataMember]
		public string LogoFilePath { get; set; }

		/// <summary>
		/// Audio type of toast (optional)
		/// </summary>
		[DataMember]
		public ToastAudio Audio { get; set; }

		/// <summary>
		/// XML representation of toast (optional)
		/// </summary>
		/// <remarks>If specified, this XML will be used for a toast as it is. The other toast elements
		/// will be ignored.</remarks>
		[DataMember]
		public string Xml { get; set; }

		/// <summary>
		/// Shortcut file name to be installed in Start menu (required for shortcut)
		/// </summary>
		[DataMember]
		public string ShortcutFileName { get; set; }

		/// <summary>
		/// Target file path of shortcut (required for shortcut)
		/// </summary>
		[DataMember]
		public string ShortcutTargetFilePath { get; set; }

		/// <summary>
		/// Arguments of shortcut (optional)
		/// </summary>
		[DataMember]
		public string ShortcutArguments { get; set; }

		/// <summary>
		/// Comment of shortcut (optional)
		/// </summary>
		[DataMember]
		public string ShortcutComment { get; set; }

		/// <summary>
		/// Working folder of shortcut (optional)
		/// </summary>
		[DataMember]
		public string ShortcutWorkingFolder { get; set; }

		/// <summary>
		/// Window state of shortcut (optional)
		/// </summary>
		[DataMember]
		public ShortcutWindowState ShortcutWindowState { get; set; }

		/// <summary>
		/// Icon file path of shortcut (optional)
		/// </summary>
		/// <remarks>If not specified, target file path will be used.</remarks>
		[DataMember]
		public string ShortcutIconFilePath
		{
			get
			{
				return !string.IsNullOrWhiteSpace(_shortcutIconFilePath)
					? _shortcutIconFilePath
					: ShortcutTargetFilePath;
			}
			set { _shortcutIconFilePath = value; }
		}
		private string _shortcutIconFilePath;

		/// <summary>
		/// AppUserModelID of application (required)
		/// </summary>
		/// <remarks>
		/// An AppUserModelID must be in the following form:
		/// CompanyName.ProductName.SubProduct.VersionInformation
		/// It can have no more than 128 characters and cannot contain spaces. Each section should be
		/// camel-cased. CompanyName and ProductName should always be used, while SubProduct and
		/// VersionInformation are optional.
		/// </remarks>
		[DataMember]
		public string AppId { get; set; }

		/// <summary>
		/// AppUserModelToastActivatorCLSID of application (optional, for Action Center of Windows 10)
		/// </summary>
		/// <remarks>This CLSID is necessary for an application to be started by COM.</remarks>
		[DataMember]
		public Guid ActivatorId { get; set; }

		/// <summary>
		/// Waiting duration before showing a toast after the shortcut file is installed (optional)
		/// </summary>
		[DataMember]
		public TimeSpan? WaitingDuration { get; set; }

        [DataMember]
        public string Group { get; set; }

        [DataMember]
        public string Tag { get; set; }

        [DataMember]
        public bool SuppressPopup { get; set; }

        #endregion

        #region Internal Property

        internal bool IsShortcutValid =>
			!string.IsNullOrWhiteSpace(AppId) &&
			!string.IsNullOrWhiteSpace(ShortcutFileName) &&
			!string.IsNullOrWhiteSpace(ShortcutTargetFilePath);
        
        internal bool IsToastValid =>
            !string.IsNullOrWhiteSpace(AppId) &&
            ((BodyList?.Any()).GetValueOrDefault() ||
             !string.IsNullOrWhiteSpace(Xml));

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ToastRequest()
		{ }

		internal ToastRequest(string requestJson) : this()
		{
			Import(requestJson);
		}

		#endregion

		#region Import/Export

		/// <summary>
		/// Imports from a request in JSON format.
		/// </summary>
		/// <param name="requestJson">Request in JSON format</param>
		internal void Import(string requestJson)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(requestJson)))
			{
				var serializer = new DataContractJsonSerializer(typeof(ToastRequest));
				var buff = (ToastRequest)serializer.ReadObject(stream);

				typeof(ToastRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(x => x.CanWrite)
					.ToList()
					.ForEach(x => x.SetValue(this, x.GetValue(buff)));
			}
		}

		/// <summary>
		/// Exports a request in JSON format.
		/// </summary>
		/// <returns>Request in JSON format</returns>
		internal string Export()
		{
			using (var stream = new MemoryStream())
			{
				var serializer = new DataContractJsonSerializer(typeof(ToastRequest));
				serializer.WriteObject(stream, this);

				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		#endregion
	}
}