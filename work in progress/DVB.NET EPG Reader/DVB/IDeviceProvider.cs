using System;
using System.Collections;

namespace JMS.DVB
{
	/// <summary>
	/// Callback interface when using PID filters.
	/// <seealso cref="IDeviceProvider.StartSectionFilter"/>
	/// </summary>
	public delegate void FilterHandler(byte[] data);

	/// <summary>
	/// Abstraction of the DVB hardware access.
	/// </summary>
	public interface IDeviceProvider: IDisposable
	{
		/// <summary>
		/// Report the type of the receiver.
		/// </summary>
		FrontendType ReceiverType { get; }

		/// <summary>
		/// Report all known stations.
		/// </summary>
		IEnumerable Stations { get; }

		/// <summary>
		/// Using a unique identification find the related station record.
		/// </summary>
		/// <param name="key">The unique identification of the station.</param>
		/// <returns>A station record or <i>null</i>.</returns>
		Station FindStation(Identifier key);
		
		/// <summary>
		/// Stop all PID filters.
		/// </summary>
		void StopFilters();

		/// <summary>
		/// Tune the DVB device to the indicated transponder.
		/// </summary>
		/// <param name="transponder">Opaque information of the transponder to use.</param>
		void Tune(Transponder transponder);

		/// <summary>
		/// Tune the DVB device to the indicated transponder with predefined DiSEqC
		/// configuration.
		/// </summary>
		/// <param name="transponder">Opaque information of the transponder to use.</param>
		/// <param name="diseqc">Optional DiSEqC configuration which will be used only
		/// for DVB-S. If this parameter is <i>null</i> <see cref="Tune(Transponder)"/>
		/// will be used.</param>
		void Tune(Transponder transponder, Satellite.DiSEqC diseqc);

		/// <summary>
		/// Set the video and primary audio PID for display and single
		/// channel recording.
		/// </summary>
		/// <param name="videoPID">The video signal to show.</param>
		/// <param name="audioPID">The audio signal to activate.</param>
		/// <param name="ac3PID">The Dolby Digital (AC3) audio signal to activate.</param>
		void SetVideoAudio(ushort videoPID, ushort audioPID, ushort ac3PID);

		/// <summary>
		/// Start a section filter.
		/// </summary>
		/// <param name="pid">PID to filter upon.</param>
		/// <param name="callback">Method to call when new data is available.</param>
		/// <param name="filterData">Filter data for pre-selection.</param>
		/// <param name="filterMask">Masks those bits in the filter data for pre-selection
		/// which are relevant for comparision.</param>
		void StartSectionFilter(ushort pid, FilterHandler callback, byte[] filterData, byte[] filterMask);

		/// <summary>
		/// Prepare filtering a DVB stream.
		/// </summary>
		/// <remarks>
		/// Use <see cref="StartFilter"/> to start filtering.
		/// </remarks>
		/// <param name="pid">PID to filter upon.</param>
		/// <param name="video">Set if a video stream is used.</param>
		/// <param name="smallBuffer">Unset if the largest possible buffer should be used.</param>
		/// <param name="callback">Method to call when new data is available.</param>
		void RegisterPipingFilter(ushort pid, bool video, bool smallBuffer, FilterHandler callback);

		/// <summary>
		/// Start filtering a DVB stream.
		/// </summary>
		/// <param name="pid">PID on which the filter runs.</param>
		void StartFilter(ushort pid);

		/// <summary>
		/// Suspend filtering a DVB stream.
		/// </summary>
		/// <param name="pid">PID on which the filter runs.</param>
		void StopFilter(ushort pid);

		/// <summary>
		/// Find the station information for the indicated station name.
		/// </summary>
		/// <remarks>
		/// If no station with the indicated name can be found on any transponder
		/// the <see cref="Array"/> reported is empty. If the transponder name is
		/// given the resulting <see cref="Array"/> includes at most a station with
		/// both names matching. If the transponder name is <i>null</i> there may
		/// be more than one station record in the return value if the station
		/// name is used inside different transponders.
		/// </remarks>
		/// <param name="station">The name of the station.</param>
		/// <param name="provider">The name of the transponder which may be <i>null</i>.</param>
		/// <returns></returns>
		Station[] ResolveStation(string station, string provider);

		/// <summary>
		/// Filter a single stream to a file.
		/// </summary>
		/// <param name="pid">Stream identifier (PID) to filter.</param>
		/// <param name="video">Set for a video stream to use largest buffer possible.</param>
		/// <param name="path">Full path to the file.</param>
		void StartFileFilter(ushort pid, bool video, string path);

		/// <summary>
		/// Retrieve the number of bytes transferred through this filter.
		/// </summary>
		/// <param name="pid">Stream identifier (PID) to filter.</param>
		/// <returns>Bytes this filter has passed through</returns>
		long GetFilterBytes(ushort pid);

		/// <summary>
		/// Start recording on the primary station - must be pre-selected
		/// by caller.
		/// </summary>
		/// <param name="path">Full path to the recorder file.</param>
		void StartRecording(string path);

		/// <summary>
		/// Start recording in PVA format.
		/// </summary>
		/// <param name="callback">PVA packets will be sent to here.</param>
		void StartPVARecording(FilterHandler callback);

		/// <summary>
		/// Stop recording on the primary station.
		/// </summary>
		void StopRecording();

		/// <summary>
		/// Get the total length of the primary recording.
		/// </summary>
		long RecordedBytes { get; }

		/// <summary>
		/// See if decryption is activated.
		/// </summary>
		bool IsDecrypting { get; }

		/// <summary>
		/// Activate decrypting the indicated station.
		/// </summary>
		/// <param name="station">Some station.</param>
		void Decrypt(Station station);

		/// <summary>
		/// Report the maximum number of PID filters available.
		/// </summary>
		int FilterLimit { get; }

		/// <summary>
		/// Report the special features supported by this provider.
		/// </summary>
		ProviderFeatures Features { get; }

		/// <summary>
		/// Report the start of a number of filter registrations.
		/// <seealso cref="EndRegister"/>
		/// </summary>
		void BeginRegister();

		/// <summary>
		/// Registration of filters finished.
		/// <seealso cref="BeginRegister"/>
		/// </summary>
		void EndRegister();

		/// <summary>
		/// Reload the channel file.
		/// </summary>
		void ReloadChannels();

		/// <summary>
		/// Get or set the current profile to use.
		/// </summary>
		IDeviceProfile Profile { get; set; }

		/// <summary>
		/// Called after a wakeup from hibernation prior to the first tune request.
		/// </summary>
		void WakeUp();

		/// <summary>
		/// Report the current status of the signal.
		/// </summary>
		SignalStatus SignalStatus { get; }
	}
}
