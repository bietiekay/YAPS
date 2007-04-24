using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace YAPS
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // clear the listview
            RecordingsOnServer.Items.Clear();
            
            // this is the url where we can find the xml feed
            string strPathName = "http://bubblegum.fem.tu-ilmenau.de/rss/recorded.xml";

            // init and get the RSS Data
            DataTable rss_data = RSS.RSSUtilities.GetRSSFeed(RSS.RSSLocation.URL,strPathName, RSS.RSSFeedType.item);

            // now add the RSS Items to the listview
            foreach (DataRow item in rss_data.DataSet.Tables["Item"].Rows)
            {
                string itemlink = (string)item.ItemArray[0];
                string itemname = (string)item.ItemArray[1];

                ListViewItem newItem = RecordingsOnServer.Items.Add(itemname, itemlink);

                // ListViewItem.ListViewSubItem SubItem = new ListViewItem.ListViewSubItem(newItem,(string)item.ItemArray[2]);

                newItem.SubItems.Add((string)item.ItemArray[2]);
            }
        }

        private void getReToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RecordingsOnServer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView lv = (ListView)sender;

            Playlist.Items.Add((ListViewItem)lv.FocusedItem.Clone());
            lv.Items.Remove(lv.FocusedItem);
        }

        private void Playlist_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView lv = (ListView)sender;

            RecordingsOnServer.Items.Add((ListViewItem)lv.FocusedItem.Clone());
            lv.Items.Remove(lv.FocusedItem);
        }

        private void playitems()
        {
            foreach (ListViewItem item in Playlist.Items)
            {
                // now spawn mplayer sessions for the playlist...
                string strCmdLine;
                strCmdLine = "-fs " + item.ImageKey;
                string originalName = item.Name;
                item.Name = item.Name + " [now playing]";

                System.Diagnostics.Process mplayerprocess;
                mplayerprocess = new System.Diagnostics.Process();

                //Do not receive an event when the process exits.
                mplayerprocess.EnableRaisingEvents = false;

                mplayerprocess = System.Diagnostics.Process.Start("mplayer.exe", strCmdLine);

                while (!mplayerprocess.HasExited)
                {
                    if (!PlaylistManager.play) break;
                    System.Threading.Thread.Sleep(10);
                }

                mplayerprocess.Close();

                item.Name = originalName;
            }

        }

        private void Play_Click(object sender, EventArgs e)
        {
            /*Play.Enabled = false;
            cancelbutton.Enabled = true;

            PlaylistManager.play = true;

            while (EndlessLoop.Checked)
            {
                playitems();
                System.Threading.Thread.Sleep(10);
            }
            if (!EndlessLoop.Checked)
                playitems();

            Play.Enabled = true;
            cancelbutton.Enabled = false;
            */
            // this is the old playlist.cmd part
            
            using (TextWriter w = File.CreateText("playlist.cmd"))
            {
                if (EndlessLoop.Checked)
                    w.WriteLine(":start");
                
                foreach(ListViewItem item in Playlist.Items)
                {
                    w.WriteLine("mplayer -fs " + item.ImageKey);
                }
                if (EndlessLoop.Checked)
                    w.WriteLine("goto start");
            }

            System.Diagnostics.Process mplayerprocess;
            mplayerprocess = new System.Diagnostics.Process();

            //Do not receive an event when the process exits.
            mplayerprocess.EnableRaisingEvents = false;

            mplayerprocess = System.Diagnostics.Process.Start("cmd.exe", "/C playlist.cmd");

            mplayerprocess.Close();
        }

        private void up_button_Click(object sender, EventArgs e)
        {
            ListViewItem item = (ListViewItem)Playlist.FocusedItem.Clone();

            // check if we're already at the top, if so, add to the bottom, if not, move one position up
            if (Playlist.FocusedItem.Index == 0)
            {
                Playlist.Items.RemoveAt(0);
                Playlist.Items.Add(item);
                Playlist.FocusedItem = item;
            }
            else
            {
                int index = Playlist.FocusedItem.Index;
                Playlist.Items.RemoveAt(index);
                Playlist.Items.Insert(index - 1, item);
                Playlist.FocusedItem = item;
            }
        }

        private void down_button_Click(object sender, EventArgs e)
        {
            ListViewItem item = (ListViewItem)Playlist.FocusedItem.Clone();

            // check if we're already at the top, if so, add to the bottom, if not, move one position up
            if (Playlist.FocusedItem.Index == Playlist.Items.Count-1)
            {
                Playlist.Items.RemoveAt(Playlist.FocusedItem.Index);
                Playlist.Items.Insert(0,item);
                Playlist.FocusedItem = item;
            }
            else
            {
                int index = Playlist.FocusedItem.Index;
                Playlist.Items.RemoveAt(index);
                Playlist.Items.Insert(index + 1, item);
                Playlist.FocusedItem = item;
            }

        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            updateToolStripMenuItem_Click(sender, e);
        }
    }
}