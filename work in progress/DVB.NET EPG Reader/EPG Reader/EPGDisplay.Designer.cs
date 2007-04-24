namespace TSSplitter
{
	partial class EPGDisplay
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lbName = new System.Windows.Forms.Label();
			this.lbTime = new System.Windows.Forms.Label();
			this.lbDescription = new System.Windows.Forms.Label();
			this.cmdCopy = new System.Windows.Forms.Button();
			this.cmdClose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lbName
			// 
			this.lbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbName.Location = new System.Drawing.Point(12, 9);
			this.lbName.Name = "lbName";
			this.lbName.Size = new System.Drawing.Size(465, 19);
			this.lbName.TabIndex = 0;
			this.lbName.Text = "label1";
			// 
			// lbTime
			// 
			this.lbTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lbTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbTime.Location = new System.Drawing.Point(12, 37);
			this.lbTime.Name = "lbTime";
			this.lbTime.Size = new System.Drawing.Size(465, 19);
			this.lbTime.TabIndex = 0;
			this.lbTime.Text = "label1";
			// 
			// lbDescription
			// 
			this.lbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lbDescription.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lbDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbDescription.Location = new System.Drawing.Point(12, 68);
			this.lbDescription.Margin = new System.Windows.Forms.Padding(5);
			this.lbDescription.Name = "lbDescription";
			this.lbDescription.Size = new System.Drawing.Size(465, 269);
			this.lbDescription.TabIndex = 0;
			this.lbDescription.Text = "label1";
			// 
			// cmdCopy
			// 
			this.cmdCopy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmdCopy.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCopy.Location = new System.Drawing.Point(12, 376);
			this.cmdCopy.Name = "cmdCopy";
			this.cmdCopy.Size = new System.Drawing.Size(465, 23);
			this.cmdCopy.TabIndex = 1;
			this.cmdCopy.Text = "&Copy";
			this.cmdCopy.UseVisualStyleBackColor = true;
			this.cmdCopy.Click += new System.EventHandler(this.cmdCopy_Click);
			// 
			// cmdClose
			// 
			this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdClose.Location = new System.Drawing.Point(12, 347);
			this.cmdClose.Name = "cmdClose";
			this.cmdClose.Size = new System.Drawing.Size(465, 23);
			this.cmdClose.TabIndex = 1;
			this.cmdClose.Text = "Copy &and Close";
			this.cmdClose.UseVisualStyleBackColor = true;
			this.cmdClose.Click += new System.EventHandler(this.cmdCopy_Click);
			// 
			// EPGDisplay
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(489, 402);
			this.Controls.Add(this.cmdClose);
			this.Controls.Add(this.cmdCopy);
			this.Controls.Add(this.lbDescription);
			this.Controls.Add(this.lbTime);
			this.Controls.Add(this.lbName);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(497, 436);
			this.Name = "EPGDisplay";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "EPGDisplay";
			this.Load += new System.EventHandler(this.EPGDisplay_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lbName;
		private System.Windows.Forms.Label lbTime;
		private System.Windows.Forms.Label lbDescription;
		private System.Windows.Forms.Button cmdCopy;
		private System.Windows.Forms.Button cmdClose;
	}
}