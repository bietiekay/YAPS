using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
	public class SatelliteDelivery: Descriptor
	{
		public readonly uint Frequency;

		public readonly uint SymbolRate;

		public readonly ushort OrbitalPosition;

		public readonly bool West;

		public readonly Polarizations Polarization;

		public readonly byte Modulation;

		public readonly InnerFECs InnerFEC;

		public SatelliteDelivery(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
		{
			// Not possible
			if (11 != length) return;

			// Attach to section
			Section section = container.Section;

			// Load parts
			uint freq0 = (uint)Tools.FromBCD(section[offset + 0]);
			uint freq1 = (uint)Tools.FromBCD(section[offset + 1]);
			uint freq2 = (uint)Tools.FromBCD(section[offset + 2]);
			uint freq3 = (uint)Tools.FromBCD(section[offset + 3]);
			ushort orb0 = (ushort)Tools.FromBCD(section[offset + 4]);
			ushort orb1 = (ushort)Tools.FromBCD(section[offset + 5]);
			uint rate0 = (uint)Tools.FromBCD(section[offset + 7]);
			uint rate1 = (uint)Tools.FromBCD(section[offset + 8]);
			uint rate2 = (uint)Tools.FromBCD(section[offset + 9]);
			uint rate3 = (uint)Tools.FromBCD((byte)(section[offset + 10] & 0xf0));

			// Flags
			byte flags = section[offset + 6];

			// Load all
			Frequency = freq3 + 100 * (freq2 + 100 * (freq1 + 100 * freq0));
			SymbolRate = (rate3 + 100 * (rate2 + 100 * (rate1 + 100 * rate0))) / 10;
			Polarization = (Polarizations)((flags >> 5) & 0x03);
			InnerFEC = (InnerFECs)(section[offset + 10] & 0x0f);
			OrbitalPosition = (ushort)(orb1 + 100 * orb0);
			West = (0x00 == (0x80 & flags));
			Modulation = (byte)(flags & 0x1f);

			// We are valid
			m_Valid = true;
		}

        public static bool IsHandlerFor(byte tag)
		{
			// Check it
			return (DescriptorTags.SatelliteDeliverySystem == (DescriptorTags)tag);
		}
	}
}
