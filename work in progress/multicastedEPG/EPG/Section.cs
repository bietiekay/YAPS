using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
	/// <summary>
	/// Instances of this class represent a single section inside the raw data
	/// of the <i>Service Information (SI)</i> in a DVB stream.
	/// </summary>
	/// <remarks>
	/// The physical layout of each section is described in the following table. For
	/// details please refer to the original documentation,
	/// e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
	/// <list type="table">
	/// <listheader><term>Byte/Bit(s)</term><description>Value/Meaning</description></listheader>
	/// <item><term>0/7-0</term><description><see cref="TableIdentifier"/></description></item>
	/// <item><term>1/7</term><description><see cref="Syntax"/> Indicator, must be <i>1</i></description></item>
	/// <item><term>1/6</term><description>Reserved for future use</description></item>
	/// <item><term>1/5-4</term><description>Reserved</description></item>
	/// <item><term>1/3-0</term><description>Bits 8 to 11 of <see cref="Length"/></description></item>
	/// <item><term>2/7-0</term><description>Bits 0 to 7 of <see cref="Length"/></description></item>
	/// </list>
	/// The indicated number of bytes follow. The last four bytes is the CRC32 checksum.
	/// </remarks>
	public class Section
	{
		/// <summary>
		/// Encoding map used to correctly map the raw data bytes to <see cref="char"/>
		/// values in the correct code page.
		/// </summary>
		/// <remarks>
		/// Currently the implementation only tries to encode the 8 bit representations
		/// from <i>0x01</i> to <i>0xf</i>. If 16 bit character coding is used the results
		/// are unpredictable.
		/// </remarks>
		static private Encoding[] m_Encodings = new Encoding[16];

		/// <summary>
		/// Default encoding to use.
		/// </summary>
		static private Encoding m_DefaultEncoding;

		/// <summary>
		/// Report if this instance has a valid checksum.
		/// <seealso cref="CRC32"/>
		/// </summary>
		public readonly bool IsValid = false;

		/// <summary>
		/// The DVB table identifier for this section.
		/// <seealso cref="Table"/>
		/// </summary>
		public readonly byte TableIdentifier;

		/// <summary>
		/// The related table instance which depends on the value
		/// of <see cref="TableIdentifier"/>.
		/// </summary>
		/// <remarks>
		/// The <see cref="EPG.Table.IsValid"/> will always be set.
		/// </remarks>
		public readonly Table Table;

		/// <summary>
		/// The syntax indicator from the raw data.
		/// </summary>
		public readonly bool Syntax;

		/// <summary>
		/// The complete length of this section instance.
		/// </summary>
		/// <remarks>
		/// This will include not only the raw data for our <see cref="Table"/>
		/// but also the sections private header. In constrast using the <see cref="this"/>
		/// indexer the byte at position <i>0</i> is always the first raw byte of
		/// the <see cref="Table"/>.
		/// </remarks>
		public readonly int Length;

		/// <summary>
		/// A copy of the full raw data for our <see cref="Table"/>.
		/// <seealso cref="Length"/>
		/// </summary>
		/// <remarks>
		/// The <see cref="Array.Length"/> of this <see cref="Array"/> will
		/// be 3 less than our <see cref="Length"/>.
		/// </remarks>
		private byte[] m_RawData;

		/// <summary>
		/// Initialize the static lookup tables.
		/// </summary>
		static Section()
		{
			// Load all encodings
			SetEncoding(0x01, "ISO-8859-5");
			SetEncoding(0x02, "ISO-8859-6");
			SetEncoding(0x03, "ISO-8859-7");
			SetEncoding(0x04, "ISO-8859-8");
			SetEncoding(0x05, "ISO-8859-9");
			SetEncoding(0x06, "ISO-8859-10");
			SetEncoding(0x07, "ISO-8859-11");
			SetEncoding(0x08, "ISO-8859-12");
			SetEncoding(0x09, "ISO-8859-13");
			SetEncoding(0x0a, "ISO-8859-14");
			SetEncoding(0x0b, "ISO-8859-15");

			// Load default
			m_DefaultEncoding = m_Encodings[0x0b];
		}

		/// <summary>
		/// Attach the indicated encoding to the corresponding entry
		/// in the lookup map.
		/// <seealso cref="m_Encodings"/>
		/// </summary>
		/// <remarks>
		/// If the <see cref="Encoding"/> can not be found using <see cref="Encoding.GetEncoding"/>
		/// the lookup map entry is left empty. When needed <see cref="Encoding.Default"/> is
		/// used as a substitute.
		/// </remarks>
		/// <param name="index">The SI code page index.</param>
		/// <param name="name">The related encoding name.</param>
		static private void SetEncoding(byte index, string name)
		{
			// Be safe
			try
			{
				// Load
				m_Encodings[index] = Encoding.GetEncoding(name);
			}
			catch
			{
				// Ignore any error
			}
		}

		/// <summary>
		/// Create a new instance.
		/// </summary>
		/// <remarks>
		/// A private copy of the raw data will be created exclusivly  for this instance.
		/// If the checksum could be validated the constructor tries to use
		/// <see cref="EPG.Table.Create"/> to create our <see cref="Table"/>. The
		/// result of the call will only be used if <see cref="EPG.Table.IsValid"/>
		/// is set.
		/// </remarks>
		/// <param name="tableIdentifier">The related table indentifier.</param>
		/// <param name="syntax">The syntax indicator.</param>
		/// <param name="rawData">Raw data from some external buffer.</param>
		/// <param name="offset">First byte of our raw data in the buffer - actually
		/// the first byte for our <see cref="Table"/>.</param>
		/// <param name="length">Number of bytes in our raw data.</param>
		/// <param name="isValid">Set if the <see cref="CRC32"/> checksum could be validated.</param>
		private Section(byte tableIdentifier, bool syntax, byte[] rawData, int offset, int length, bool isValid)
		{
			// Remember
			TableIdentifier = tableIdentifier;
			Length = 3 + length;
			IsValid = isValid;
			Syntax = syntax;

			// Create helper
			m_RawData = new byte[length];

			// Fill helper
			Array.Copy(rawData, offset, m_RawData, 0, length);

			// Don't try to create table
			if (!IsValid) return;

			// Create the table
			Table = EPG.Table.Create(this);

			// Discard
			if (!Table.IsValid) Table = null;
		}

		/// <summary>
		/// Create a new section from a raw data buffer.
		/// <seealso cref="Section"/>
		/// </summary>
		/// <param name="buffer">The full buffer.</param>
		/// <param name="offset">The first byte of the data for the section.</param>
		/// <param name="length">The number of bytes available for the section.</param>
		/// <returns><i>null</i> if there are not enough bytes available in
		/// the raw data for the section. If a <see cref="Section"/> is reported
		/// it may have <see cref="Section.IsValid"/> unset to indicated that
		/// the <see cref="CRC32"/> validation failed.</returns>
		static internal Section Create(byte[] buffer, int offset, int length)
		{
			// Check for the minimum size
			if (length < 7) return null;

			// Decode
			byte tableIdentifier = buffer[offset + 0];
			byte flags = buffer[offset + 1];
			int lowLength = buffer[offset + 2];

			// Decode flags
			bool syntax = (0 != (0x80 & flags));
			int highLength = flags & 0xf;

			// Construct the overall size
			int size = lowLength + 256 * highLength;

			// Verify
			if ((3 + size) > length) return null;

			// Create the new section
			return new Section(tableIdentifier, syntax, buffer, offset + 3, size, CRC32.CheckCRC(buffer, offset, 3 + size));
		}

		/// <summary>
		/// Directly access a raw data byte.
		/// <seealso cref="Length"/>
		/// </summary>
		/// <param name="index">A zero-based index into our private data. The
		/// maximum value allowed is <see cref="Length"/> <i>- 4</i>.</param>
		public byte this[int index]
		{
			get
			{
				// Report
				return m_RawData[index];
			}
		}

		/// <summary>
		/// Directly access a number of bytes in our raw data.
		/// </summary>
		/// <param name="offset">A zero-based index into our private data. The
		/// maximum value allowed is <see cref="Length"/> <i>- 4</i>.</param>
		/// <param name="bytes">Number of bytes to return.</param>
		/// <returns>A copy of the indicated range.</returns>
		public byte[] ReadBytes(int offset, int bytes)
		{
			// Create
			byte[] ret = new byte[bytes];

			// Fill
			Array.Copy(m_RawData, offset, ret, 0, bytes);

			// Report
			return ret;
		}

		/// <summary>
		/// Use the <see cref="Encoding.ASCII"/> <see cref="Encoding"/> to
		/// read the indicated number of bytes to a <see cref="string"/>.
		/// <seealso cref="Encoding.GetString"/>
		/// </summary>
		/// <param name="offset">A zero-based index into our private data. The
		/// maximum value allowed is <see cref="Length"/> <i>- 4</i>.</param>
		/// <param name="bytes">Number of characters to read.</param>
		/// <returns>The corresponding <see cref="string"/>.</returns>
		public string ReadString(int offset, int bytes)
		{
			// The fast way
			return ReadEncodedString(Encoding.ASCII, offset, bytes, false);
		}

		/// <summary>
		/// Read an encoded string from the raw data. The first <see cref="byte"/>
		/// may be a code page selector.
		/// </summary>
		/// <remarks>
		/// If the code page selector is between <i>0x10</i> and <i>0x1f</i> the
		/// result is unpredictable. For more details please refer to the original
		/// documentation, e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
		/// </remarks>
		/// <param name="offset">A zero-based index into our private data. The
		/// maximum value allowed is <see cref="Length"/> <i>- 4</i>.</param>
		/// <param name="bytes">Number of characters to read.</param>
		/// <returns>The corresponding <see cref="string"/>.</returns>
		public string ReadEncodedString(int offset, int bytes)
		{
			// Forward
			return ReadEncodedString(offset, bytes, false);
		}

		/// <summary>
		/// Read an encoded string from the raw data.
		/// </summary>
		/// <param name="encoding">Encoding to use for character translation.</param>
		/// <param name="offset">A zero-based index into our private data. The
		/// maximum value allowed is <see cref="Length"/> <i>- 4</i>.</param>
		/// <param name="bytes">Number of characters to read.</param>
		/// <param name="removeSpecial">Set to remove control characters instead
		/// of replacing it by blanks.</param>
		/// <returns>The corresponding <see cref="string"/>.</returns>
		public string ReadEncodedString(Encoding encoding, int offset, int bytes, bool removeSpecial)
		{
			// Allocate buffer
			List<byte> scratch = new List<byte>(bytes);

			// Process all
			while (bytes-- > 0)
			{
				// Load
				byte ch = m_RawData[offset++];

				// Replace special characters
				if (0x8a == ch)
				{
					// Line feed
					scratch.Add(0x0a);
					scratch.Add(0x0d);
				}
				else if (((ch >= 0x00) && (ch <= 0x1f)) || ((ch >= 0x80) && (ch <= 0x9f)))
				{
					// Control
					if (!removeSpecial) scratch.Add(0x20);
				}
				else
				{
					// As is
					scratch.Add(ch);
				}
			}

			// Process
			return encoding.GetString(scratch.ToArray());
		}

		/// <summary>
		/// Read an encoded string from the raw data. The first <see cref="byte"/>
		/// may be a code page selector.
		/// </summary>
		/// <remarks>
		/// If the code page selector is between <i>0x10</i> and <i>0x1f</i> the
		/// result is unpredictable. For more details please refer to the original
		/// documentation, e.g. <i>ETSI EN 300 468 V1.6.1 (2004-06)</i> or alternate versions.
		/// </remarks>
		/// <param name="offset">A zero-based index into our private data. The
		/// maximum value allowed is <see cref="Length"/> <i>- 4</i>.</param>
		/// <param name="bytes">Number of characters to read.</param>
		/// <param name="removeSpecial">Set to remove control characters instead
		/// of replacing it by blanks.</param>
		/// <returns>The corresponding <see cref="string"/>.</returns>
		public string ReadEncodedString(int offset, int bytes, bool removeSpecial)
		{
			// No data
			if (bytes < 1) return string.Empty;

			// Check the code page identifier
			byte cpid = m_RawData[offset];

			// Check for prefix
			if ((cpid < m_Encodings.Length) && (bytes < 2)) return string.Empty;

			// Load the encoding
			Encoding enc = (cpid < m_Encodings.Length) ? m_Encodings[cpid] : null;

			// Load default
			if (null == enc) enc = m_DefaultEncoding;

			// Check mode
			int delta = (cpid < m_Encodings.Length) ? 1 : 0;

			// Forward
			return ReadEncodedString(enc, offset + delta, bytes - delta, removeSpecial);
		}
	}
}
