using System;

namespace JMS.DVB.EPG
{
	/// <summary>
	/// A single entry in a <see cref="Tables.PMT"/>.
	/// </summary>
	public class ProgramEntry: EntryBase
	{
		/// <summary>
		/// The <see cref="Descriptor"/> instances related to this program.
		/// </summary>
		/// <remarks>
		/// Please refer to the original documentation to find out which descriptor
		/// type is allowed in a <see cref="Tables.PMT"/> table.
		/// </remarks>
		public readonly Descriptor[] Descriptors;

		/// <summary>
		/// Set if the program entry is consistent.
		/// </summary>
		public readonly bool IsValid = false;

		/// <summary>
		/// The total length of the entry in bytes.
		/// </summary>
		public readonly int Length;

        /// <summary>
        /// The PID of this program entry.
        /// </summary>
        public ushort ElementaryPID;

        /// <summary>
        /// The type of this stream
        /// </summary>
        public StreamTypes StreamType;

		/// <summary>
		/// Create a new program instance.
		/// </summary>
		/// <param name="table">The related <see cref="Tables.PMT"/> table.</param>
		/// <param name="offset">The first byte of this program in the <see cref="EPG.Table.Section"/>
		/// for the related <see cref="Table"/>.</param>
		/// <param name="length">The maximum number of bytes available. If this number
		/// is greater than the <see cref="Length"/> of this program another event will
		/// follow in the same table.</param>
        internal ProgramEntry(Table table, int offset, int length)
            : base(table)
		{
			// Access section
			Section section = Section;

            // Load
            ElementaryPID = (ushort)(0x1fff & Tools.MergeBytesToWord(section[offset + 2], section[offset + 1]));
            StreamType = (StreamTypes)section[offset + 0];

			// Read the length
            int descrLength = 0xfff & Tools.MergeBytesToWord(section[offset + 4], section[offset + 3]);

			// Caluclate the total length
            Length = 5 + descrLength;

			// Verify
			if ( Length > length ) return;

			// Try to load descriptors
            Descriptors = Descriptor.Load(this, offset + 5, descrLength);

			// Can use it
			IsValid = true;
		}

		/// <summary>
		/// Create a new program instance.
		/// </summary>
		/// <param name="table">The related <see cref="Tables.PMT"/> table.</param>
		/// <param name="offset">The first byte of this service in the <see cref="EPG.Table.Section"/>
		/// for the related <see cref="Table"/>.</param>
		/// <param name="length">The maximum number of bytes available. If this number
		/// is greater than the <see cref="Length"/> of this program entry another entry will
		/// follow in the same table.</param>
		/// <returns>A new service instance or <i>null</i> if there are less than
		/// 5 bytes available.</returns>
        static internal ProgramEntry Create(Table table, int offset, int length)
		{
			// Validate
			if ( length < 5 ) return null;

			// Create
            return new ProgramEntry(table, offset, length);
		}
	}
}
