using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace JMS.DVB
{
	/// <summary>
	/// Summary description for ProviderChooser.
	/// </summary>
	internal class ProviderChooser : System.Windows.Forms.Form
	{
		private DeviceInformations m_Devices;

		private System.Windows.Forms.ComboBox selDevice;
		private System.Windows.Forms.ListBox lstDetails;
		private System.Windows.Forms.Button cmdSave;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProviderChooser(DeviceInformations devices)
		{
			// Remember
			m_Devices = devices;

			// Required for Windows Form Designer support
			InitializeComponent();

			// Finish
			Text = string.Format(Text, Tools.ProductName);

			// Load from list
			foreach ( DeviceInformation device in devices ) selDevice.Items.Add(device.UniqueIdentifier);
		
			// Load default
			DeviceInformation active = devices.ActiveProvider;

			// None
			if ( null == active ) return;

			// Events off
			selDevice.Enabled = false;

			// Select it
			selDevice.SelectedItem = active.UniqueIdentifier;

			// Events on
			selDevice.Enabled = true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProviderChooser));
			this.selDevice = new System.Windows.Forms.ComboBox();
			this.lstDetails = new System.Windows.Forms.ListBox();
			this.cmdSave = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// selDevice
			// 
			resources.ApplyResources(this.selDevice, "selDevice");
			this.selDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.selDevice.Name = "selDevice";
			this.selDevice.Sorted = true;
			this.selDevice.SelectedIndexChanged += new System.EventHandler(this.selDevice_SelectedIndexChanged);
			// 
			// lstDetails
			// 
			resources.ApplyResources(this.lstDetails, "lstDetails");
			this.lstDetails.Name = "lstDetails";
			this.lstDetails.Sorted = true;
			// 
			// cmdSave
			// 
			resources.ApplyResources(this.cmdSave, "cmdSave");
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// ProviderChooser
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.lstDetails);
			this.Controls.Add(this.selDevice);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ProviderChooser";
			this.Load += new System.EventHandler(this.ProviderChooser_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void ProviderChooser_Load(object sender, System.EventArgs e)
		{
		}

		private void selDevice_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Attach to the device
			DeviceInformation device = m_Devices[(string)selDevice.SelectedItem];

			// Clear
			lstDetails.Items.Clear();

			// Fill
			foreach ( string name in device.Names ) lstDetails.Items.Add(name);
		}

		private void cmdSave_Click(object sender, System.EventArgs e)
		{
			// Read 
			string id = (string)selDevice.SelectedItem;

			// Send update command
			m_Devices.ChangeActive((null == id) ? null : m_Devices[id]);

			// Done
			Close();
		}
	}
}
