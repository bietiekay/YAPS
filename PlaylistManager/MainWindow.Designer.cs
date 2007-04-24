namespace YAPS
{
    partial class MainWindow
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getReToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.RecordingsOnServer = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.up_button = new System.Windows.Forms.Button();
            this.Play = new System.Windows.Forms.Button();
            this.EndlessLoop = new System.Windows.Forms.CheckBox();
            this.down_button = new System.Windows.Forms.Button();
            this.Playlist = new System.Windows.Forms.ListView();
            this.recording = new System.Windows.Forms.ColumnHeader();
            this.recorded = new System.Windows.Forms.ColumnHeader();
            this.menuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateToolStripMenuItem,
            this.getReToolStripMenuItem,
            this.infoToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(750, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // updateToolStripMenuItem
            // 
            this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
            this.updateToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.updateToolStripMenuItem.Text = "Update";
            this.updateToolStripMenuItem.Click += new System.EventHandler(this.updateToolStripMenuItem_Click);
            // 
            // getReToolStripMenuItem
            // 
            this.getReToolStripMenuItem.Enabled = false;
            this.getReToolStripMenuItem.Name = "getReToolStripMenuItem";
            this.getReToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.getReToolStripMenuItem.Text = "Settings";
            this.getReToolStripMenuItem.Click += new System.EventHandler(this.getReToolStripMenuItem_Click);
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Enabled = false;
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.infoToolStripMenuItem.Text = "Info";
            this.infoToolStripMenuItem.Click += new System.EventHandler(this.infoToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 521);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(750, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(750, 497);
            this.splitContainer1.SplitterDistance = 353;
            this.splitContainer1.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.RecordingsOnServer);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(353, 497);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Recordings on Server";
            // 
            // RecordingsOnServer
            // 
            this.RecordingsOnServer.BackColor = System.Drawing.SystemColors.Window;
            this.RecordingsOnServer.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.recording,
            this.recorded});
            this.RecordingsOnServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecordingsOnServer.Location = new System.Drawing.Point(3, 16);
            this.RecordingsOnServer.Name = "RecordingsOnServer";
            this.RecordingsOnServer.Size = new System.Drawing.Size(347, 478);
            this.RecordingsOnServer.TabIndex = 0;
            this.RecordingsOnServer.UseCompatibleStateImageBehavior = false;
            this.RecordingsOnServer.View = System.Windows.Forms.View.Details;
            this.RecordingsOnServer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RecordingsOnServer_MouseDoubleClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.Playlist);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(393, 497);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Playlist";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.up_button);
            this.groupBox3.Controls.Add(this.Play);
            this.groupBox3.Controls.Add(this.EndlessLoop);
            this.groupBox3.Controls.Add(this.down_button);
            this.groupBox3.Location = new System.Drawing.Point(3, 441);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(387, 50);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Play";
            // 
            // up_button
            // 
            this.up_button.Location = new System.Drawing.Point(6, 19);
            this.up_button.Name = "up_button";
            this.up_button.Size = new System.Drawing.Size(33, 23);
            this.up_button.TabIndex = 4;
            this.up_button.Text = "UP";
            this.up_button.UseVisualStyleBackColor = true;
            this.up_button.Click += new System.EventHandler(this.up_button_Click);
            // 
            // Play
            // 
            this.Play.Location = new System.Drawing.Point(333, 19);
            this.Play.Name = "Play";
            this.Play.Size = new System.Drawing.Size(48, 23);
            this.Play.TabIndex = 2;
            this.Play.Text = "play";
            this.Play.UseVisualStyleBackColor = true;
            this.Play.Click += new System.EventHandler(this.Play_Click);
            // 
            // EndlessLoop
            // 
            this.EndlessLoop.AutoSize = true;
            this.EndlessLoop.Location = new System.Drawing.Point(126, 23);
            this.EndlessLoop.Name = "EndlessLoop";
            this.EndlessLoop.Size = new System.Drawing.Size(91, 17);
            this.EndlessLoop.TabIndex = 3;
            this.EndlessLoop.Text = "Loop Playlist?";
            this.EndlessLoop.UseVisualStyleBackColor = true;
            // 
            // down_button
            // 
            this.down_button.Location = new System.Drawing.Point(45, 19);
            this.down_button.Name = "down_button";
            this.down_button.Size = new System.Drawing.Size(75, 23);
            this.down_button.TabIndex = 5;
            this.down_button.Text = "DOWN";
            this.down_button.UseVisualStyleBackColor = true;
            this.down_button.Click += new System.EventHandler(this.down_button_Click);
            // 
            // Playlist
            // 
            this.Playlist.AllowDrop = true;
            this.Playlist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Playlist.Location = new System.Drawing.Point(3, 16);
            this.Playlist.Name = "Playlist";
            this.Playlist.Size = new System.Drawing.Size(387, 422);
            this.Playlist.TabIndex = 0;
            this.Playlist.UseCompatibleStateImageBehavior = false;
            this.Playlist.View = System.Windows.Forms.View.List;
            this.Playlist.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Playlist_MouseDoubleClick);
            // 
            // recording
            // 
            this.recording.Text = "Name";
            this.recording.Width = 204;
            // 
            // recorded
            // 
            this.recorded.Text = "recorded";
            this.recorded.Width = 136;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 543);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "YAPS Playlist Manager";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem getReToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView Playlist;
        private System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;
        private System.Windows.Forms.ListView RecordingsOnServer;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.CheckBox EndlessLoop;
        private System.Windows.Forms.Button Play;
        private System.Windows.Forms.Button down_button;
        private System.Windows.Forms.Button up_button;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ColumnHeader recording;
        private System.Windows.Forms.ColumnHeader recorded;
    }
}

