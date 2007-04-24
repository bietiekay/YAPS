using System;
using Microsoft.Win32;
using System.Threading;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Windows.Forms;
 
namespace JMS.DVB
{
	/// <summary>
	/// Various utilities.
	/// </summary>
	public sealed class Tools
	{
		/// <summary>
		/// The path into the registry where additional provider configuration is stored.
		/// </summary>
		public static string RegistryPath = @"SOFTWARE\JMS\DVB.NET";

		/// <summary>
		/// If set when the device selector is used it will open a dialog
		/// to choose a DVB.NET hardware abstraction.
		/// </summary>
		public static bool ForceSelection = false;

		/// <summary>
		/// Show the product name in the title of the provider chooser dialog.
		/// </summary>
		public static string ProductName = "DVB.NET";

		/// <summary>
		/// Do not allow to create instances.
		/// </summary>
		private Tools()
		{
		}

		public static IDeviceProvider CreateProvider()
		{
			// Forward
			return CreateProvider(null);
		}

		public static IDeviceProvider CreateProvider(string additionalSettings)
		{
			// Load settings
			string type = ConfigurationManager.AppSettings["DVBProvider"];
            string args = ConfigurationManager.AppSettings["DVBProviderConfig"];

			// Use default
			if (null == type) type = "JMS.ChannelManagement.ProfileSelector, JMS.ChannelManagement";

			// Merge
			if (!string.IsNullOrEmpty(additionalSettings))
				if (string.IsNullOrEmpty(args))
					args = additionalSettings;
				else
					args = args + ";" + additionalSettings;

			// Argument map
			Hashtable argMap = new Hashtable();

			// Create map from args
			if ( null != args )
				foreach ( string arg in args.Split(';') )
				{
					// Cleanup
					string assign = arg.Trim();

					// Split
					int n = assign.IndexOf('=');

					// Process
					if ( n < 0 )
					{
						// Set as flag
						argMap[assign] = true;
					}
					else
					{
						// Set as string
						argMap[assign.Substring(0, n)] = assign.Substring(n + 1);
					}
				}

			// Create
			object instance = Activator.CreateInstance(Type.GetType(type), new object[] { argMap });

			// See if this is a factory
			IDeviceProviderFactory factory = instance as IDeviceProviderFactory;

			// Forward to factory
			if ( null != factory ) return factory.Create();

			// Must be a hardware abstraction provider
			return (IDeviceProvider)instance;
		}

		/// <summary>
		/// Run some application dynamically.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		/// <param name="form">Type fo the form.</param>
		public static void RunDVBApplication(string[] args, string form)
		{
			// Check for language
			if ( args.Length > 0 )
				try
				{
					// Set language
					Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(args[0]);
				}
				catch
				{
				}

			// Dynamically attach to device
			using (IDeviceProvider device = CreateProvider()) 			
				if (null != device)
					RunDVBApplication(form, device);
		}

		/// <summary>
		/// Run some application dynamically.
		/// </summary>
		/// <param name="form">Type fo the form.</param>
		/// <param name="argument">Argument for constructor.</param>
		public static void RunDVBApplication(string form, object argument)
		{
			// Prepare
			Application.EnableVisualStyles();

			// May fail in case we already showed the selection dialog
			try
			{
				// Save call
				Application.SetCompatibleTextRenderingDefault(false);
			}
			catch
			{
				// Ignore any error
			}

			// The window to open
			Form app = (Form)Activator.CreateInstance(Type.GetType(form), new object[] { argument });

			// Run form
			Application.Run(app);
		}

		internal static RegistryKey GetConfiguration(bool writable)
		{
			// Attach to registry
			RegistryKey key = Registry.CurrentUser.OpenSubKey(Tools.RegistryPath, writable);

			// Found
			if ( null != key ) return key;

			// Try system
			return Registry.LocalMachine.OpenSubKey(Tools.RegistryPath, writable);
		}
	}
}
