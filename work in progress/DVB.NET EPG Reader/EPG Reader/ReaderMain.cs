using System;
using System.IO;
using System.Data;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using EPG = JMS.DVB.EPG;

namespace TSSplitter
{
	public partial class ReaderMain : Form
	{
		/// <summary>
		/// Size of a single packet.
		/// </summary>
		private const int PacketSize = 188;

		/// <summary>
		/// Our buffer size.
		/// </summary>
		private const int PacketsInBuffer = 100000;

		public int SortIndex = +2;

		private Dictionary<string, bool> m_Entries = new Dictionary<string, bool>();
		private List<EPGEntry> m_ListItems = new List<EPGEntry>();
		private EPG.Parser EPGParser = new EPG.Parser(null);
		private List<byte[]> m_Parts = new List<byte[]>();
		private bool m_Loading = false;
		private bool m_Reading = false;
		private FileInfo File = null;
		private int m_BytesLeft = 0;
		private int m_Counter = -1;
		private string m_StopText;
		private string m_LoadText;

		public ReaderMain(string[] args)
		{
			// Load file
			if (args.Length > 0) File = new FileInfo(args[0]);

			// Setup form
			InitializeComponent();

			// Load
			m_StopText = cmdStop.Text;
			m_LoadText = Properties.Resources.Reload;

			// Connect
			EPGParser.SectionFound += new EPG.Parser.SectionFoundHandler(SectionFound);
		}

		[STAThread]
		public static void Main(string[] args)
		{
			// Prepare GUI
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ReaderMain(args));
		}

		private void ReaderMain_Load(object sender, EventArgs e)
		{
			// Ask use
			if (null == File)
			{
				// Ask
				if (DialogResult.OK == openInput.ShowDialog(this)) File = new FileInfo(openInput.FileName);
			}

			// Set starter
			starter.Enabled = true;
		}

		private void starter_Tick(object sender, EventArgs e)
		{
			// Disable
			starter.Enabled = false;

			// Finsih
			if (null == File)
			{
				// Stop
				Close();

				// Done
				return;
			}

			// Set mode
			m_Loading = true;

			// May stop
			cmdStop.Enabled = true;

			// Reset
			m_ListItems.Clear();
			m_Entries.Clear();
			m_Reading = false;
			m_Parts.Clear();
			m_BytesLeft = 0;
			m_Counter = -1;
			
			// Be safe
			try
			{
				// The mode
				bool TSMode = Equals(File.Extension.ToLower(), ".ts");

				// Blocksize
				byte[] Buffer = new byte[TSMode ? (PacketSize * PacketsInBuffer) : 100000];

				// Open the file
				using (FileStream read = new FileStream(File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, Buffer.Length))
				{
					// Skip junk
					/*if (TSMode)
					{
						// Start at
						int start = 0;

						// Read head
						int head = read.Read(Buffer, 0, 100 * PacketSize);

						// Find the real start
						for (int last = Math.Min(PacketSize, head); start < last; ++start)
						{
							// This is not a candidate
							if (0x47 != Buffer[start]) continue;

							// Not found
							bool isStart = true;

							// Try load
							for (int test = start; isStart && ((test += PacketSize) < head); )
							{
								// Execute the test
								isStart = (0x47 == Buffer[test]);
							}

							// Found
							if (isStart) break;
						}

						// Reposition
						read.Seek(start, SeekOrigin.Begin);
					}*/

					// Content
					for (int n; (n = read.Read(Buffer, 0, Buffer.Length)) > 0; )
					{
						// Report progress
						progress.Value = (int)(read.Position * progress.Maximum / read.Length);

						// Show up
						Application.DoEvents();

						// Done
						if (!cmdStop.Enabled) break;

						// Check mode
						if (TSMode)
						{
							// Pakets
							for (int i = 0; (i + PacketSize) <= n; i += PacketSize) ProcessPacket(Buffer, i);
						}
						else
						{
							// SI Table
							EPGParser.OnData(Buffer, 0, n);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Report
				MessageBox.Show(this, ex.Message);
			}
			finally
			{
				// Done
				m_Loading = false;
			}

			// Prepare load
			cmdStop.Text = m_LoadText;
			cmdStop.Enabled = true;

			// Load all we found
			lstEntries.Items.Clear();
			lstEntries.Items.AddRange(m_ListItems.ToArray());

			// Prepare sorter
			lstEntries.ListViewItemSorter = new EPGEntry.Comparer();
		}

		private void cmdStop_Click(object sender, EventArgs e)
		{
			// Check mode
			if (!m_Loading)
			{
				// Ask
				if (DialogResult.OK != openInput.ShowDialog(this)) return;
				
				// Reload
				File = new FileInfo(openInput.FileName);

				// Set starter
				starter.Enabled = true;

				// Set text
				cmdStop.Text = m_StopText;
			}

			// Once
			cmdStop.Enabled = false;
		}

		private void SectionFound(EPG.Section section)
		{
			// Check
			if ((null == section) || !section.IsValid) return;

			// Convert
			EPG.Tables.EIT epgTable = section.Table as EPG.Tables.EIT;

			// Check it
			if ((null == epgTable) || !epgTable.IsValid) return;

			// Process all events
			foreach (EPG.EventEntry entry in epgTable.Entries)
				if (EPG.EventStatus.Running == entry.Status)
					AddEntry(epgTable.ServiceIdentifier, entry);
		}

		private void AddEntry(ushort service, EPG.EventEntry entry)
		{
			// Create a key
			string simpleKey = string.Format("{0}-{1}", service, entry.EventIdentifier);

			// Already collected
			if (m_Entries.ContainsKey(simpleKey)) return;

			// Lock out
			m_Entries[simpleKey] = true;

			// Descriptors we can have
			EPG.Descriptors.ShortEvent shortEvent = null;

			// Extended events
			List<EPG.Descriptors.ExtendedEvent> exEvents = new List<EPG.Descriptors.ExtendedEvent>();

			// Check all descriptors
			foreach (EPG.Descriptor descr in entry.Descriptors)
				if (descr.IsValid)
				{
					// Check type
					if (null == shortEvent)
					{
						// Read
						shortEvent = descr as EPG.Descriptors.ShortEvent;

						// Done for now
						if (null != shortEvent) continue;
					}

					// Test
					EPG.Descriptors.ExtendedEvent exEvent = descr as EPG.Descriptors.ExtendedEvent;

					// Register
					if (null != exEvent) exEvents.Add(exEvent);
				}

			// Data
			string name = null, description = null;

			// Take the best we got
			if (exEvents.Count > 0)
			{
				// Text builder
				StringBuilder text = new StringBuilder();

				// Process all
				foreach (EPG.Descriptors.ExtendedEvent exEvent in exEvents)
				{
					// Normal
					if (null == name) name = exEvent.Name;

					// Merge
					if (exEvent.Text != null) text.Append(exEvent.Text);
				}

				// Use
				description = text.ToString();
			}

			// Try short event
			if (null != shortEvent)
			{
				// Read
				if (null == name) name = shortEvent.Name;
				if (null == description) description = shortEvent.Text;

				// Check for additional information
				if (string.IsNullOrEmpty(name))
					name = shortEvent.Text;
				else if (!string.IsNullOrEmpty(shortEvent.Text))
					name += string.Format(" ({0})", shortEvent.Text);
			}

			// Remember
			m_ListItems.Add(new EPGEntry(service, name, description, entry.StartTime.ToLocalTime(), entry.Duration));
		}

		private void ProcessPacket(byte[] Buffer, int i)
		{
			// Test
			if (0x47 != Buffer[i++]) throw new ArgumentException("not a TS package");

			// Decode flag
			bool fstp = (0x40 == (0x40 & Buffer[i]));

			// Fast look
			int pidh = Buffer[i++] & 0x1f;
			int pidl = Buffer[i++];
			int pid = pidl + 256 * pidh;

			// Skip
			if (0x12 != pid) return;

			// Decode all (slow)
			int adap = (Buffer[i] >> 4) & 0x3;
			int counter = Buffer[i++] & 0xf;

			// Not supported by VCR.NET
			if (0 == adap) throw new ArgumentException("expected adaption or payload");

			// Check mode
			if (fstp)
			{
				// Running
				m_Reading = true;
			}
			else if (!m_Reading)
			{
				// Wait for start
				return;
			}

			// EPG should not have any adaption present
			if (2 == (2 & adap)) throw new InvalidOperationException("unexpected adaption");

			// Correct very first call
			if (m_Counter < 0)
			{
				// Start it up
				m_Counter = counter;
			}
			else if (counter != m_Counter)
			{
				// Clear buffer
				m_BytesLeft = 0;
				m_Parts.Clear();

				// Must start from scratch
				if (!fstp)
				{
					// Skip mode
					m_Reading = false;

					// Reset to beginning
					m_Counter = -1;

					// Done so far
					return;
				}

				// Reset to where we are
				m_Counter = counter;
			}

			// Count only if payload is present
			m_Counter = (m_Counter + 1) & 0xf;

			// Get the number of payload bytes
			int payload = 184, start = i;

			// Adjust
			if (fstp)
			{
				// Start a new paket
				m_BytesLeft = 0;
				m_Parts.Clear();

				// Load pointer field
				int skip = Buffer[start++];

				// Validate
				if (--payload < skip) throw new InvalidOperationException("pointer to large");

				// Adjust
				start += skip;
				payload -= skip;
			}

			// Read or correct the length
			if (0 == m_BytesLeft)
			{
				// Validate header
				if (payload < 3) throw new InvalidOperationException("corrupted (header)");

				// Decode
				int lowLength = Buffer[start + 2], highLength = Buffer[start + 1] & 0xf;

				// Construct the overall size
				int size = lowLength + 256 * highLength;

				// Set the counter
				m_BytesLeft = 3 + size;
			}

			// Correct
			if (payload > m_BytesLeft) payload = m_BytesLeft;

			// Correct
			m_BytesLeft -= payload;

			// Duplicate
			byte[] part = new byte[payload];

			// Fill
			Array.Copy(Buffer, start, part, 0, part.Length);

			// Remember
			m_Parts.Add(part);

			// Time to send
			if (m_BytesLeft < 1) SendTable();
		}

		private void selService_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void lstEntries_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			// Get the sort index
			int sortIndex = SortIndex;
			bool ascending = (sortIndex > 0);

			// Correct
			if (!ascending) sortIndex = -sortIndex;

			// New index
			int newSortIndex = e.Column + 1;

			// Check mode
			if (newSortIndex == sortIndex)
			{
				// Turn direction
				SortIndex = -SortIndex;
			}
			else
			{
				// Use as is
				SortIndex = newSortIndex;
			}

			// Resort
			lstEntries.Sort();
		}

		private void lstEntries_DoubleClick(object sender, EventArgs e)
		{
			// Check
			if (1 != lstEntries.SelectedItems.Count) return;

			// Load
			EPGEntry entry = lstEntries.SelectedItems[0] as EPGEntry;

			// None
			if (null == entry) return;

			// Show
			using (EPGDisplay dialog = new EPGDisplay(entry))
				if (DialogResult.OK == dialog.ShowDialog(this))
					Close();
		}

		private void SendTable()
		{
			// Process all
			foreach (byte[] part in m_Parts) EPGParser.OnData(part);

			// Clear
			m_Parts.Clear();
		}
	}
}