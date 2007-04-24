using System;
using System.Collections.Generic;

namespace JMS.DVB.EPG.Descriptors
{
	public class CableDelivery: Descriptor
	{
		public readonly uint Frequency;

		public readonly uint SymbolRate;

		public readonly OuterFECs OuterFEC;

		public readonly CableModulations Modulation;

		public readonly InnerFECs InnerFEC;

		public CableDelivery(IDescriptorContainer container, int offset, int length)
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
			uint rate0 = (uint)Tools.FromBCD(section[offset + 7]);
			uint rate1 = (uint)Tools.FromBCD(section[offset + 8]);
			uint rate2 = (uint)Tools.FromBCD(section[offset + 9]);
			uint rate3 = (uint)Tools.FromBCD((byte)(section[offset + 10] & 0xf0));

			// Load all
			SymbolRate = (rate3 + 100 * (rate2 + 100 * (rate1 + 100 * rate0))) / 10;
			Frequency = freq3 + 100 * (freq2 + 100 * (freq1 + 100 * freq0));
			InnerFEC = (InnerFECs)(section[offset + 10] & 0x0f);
			Modulation = (CableModulations)section[offset + 6];
			OuterFEC = (OuterFECs)(section[offset + 5] & 0x0f);

			// We are valid
			m_Valid = true;
		}

        public static bool IsHandlerFor(byte tag)
		{
			// Check it
			return (DescriptorTags.CableDeliverySystem == (DescriptorTags)tag);
		}
	}
}
