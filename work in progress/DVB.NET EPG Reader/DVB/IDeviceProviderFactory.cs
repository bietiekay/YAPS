using System;

namespace JMS.DVB
{
	/// <summary>
	/// Use for dynamic creation of hardware abstractions.
	/// </summary>
	public interface IDeviceProviderFactory
	{
		/// <summary>
		/// Create a new device.
		/// </summary>
		/// <remarks>
		/// Configuration settings are already reported through the dynamic constructor.
		/// </remarks>
		/// <returns>A newly created and pre-configured DVB abstraction.</returns>
		IDeviceProvider Create();
	}
}
