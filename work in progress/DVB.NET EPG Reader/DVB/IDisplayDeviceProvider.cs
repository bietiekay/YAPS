using System;
using System.Windows.Forms;

namespace JMS.DVB
{
	/// <summary>
	/// Implemented by a hardware abstraction if video display is supported.
	/// </summary>
	public interface IDisplayDeviceProvider: IDeviceProvider
	{
		/// <summary>
		/// Set the picture parameters of the current video.
		/// </summary>
		/// <param name="brightness">Brightness.</param>
		/// <param name="contrast">Contrast.</param>
		/// <param name="saturation">Saturation.</param>
		void SetPictureParameters(byte brightness, byte contrast, byte saturation);

		/// <summary>
		/// Create a new video window.
		/// </summary>
		/// <param name="parent">Parent window.</param>
		/// <param name="x">Left edge.</param>
		/// <param name="y">Top edge.</param>
		/// <param name="width">Width in pixels.</param>
		/// <param name="height">Height in pixels.</param>
		/// <param name="mode">Draw mode - if supported.</param>
		/// <returns>A reference to the video window.</returns>
		NativeWindow CreateVideoWindow(Control parent, int x, int y, int width, int height, string mode);

		/// <summary>
		/// Change the audio volume.
		/// </summary>
		byte Volume { set; }
	}
}
