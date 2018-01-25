using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using Windows.System.Profile;

namespace CompositionSampleGallery.Shared
{
    internal static class AppTelemetryClient
    {
        private static TelemetryClient _telemetryClient = null;
        private static string _sessionId = null;
        private static string _machineId = Guid.Empty.ToString();
        private static bool _autoFlushEvents = true;

        public static bool AutoFlushEvents { get => _autoFlushEvents; set => _autoFlushEvents = value; }

        static AppTelemetryClient()
        {
            try
            {
                TelemetryConfiguration.Active.TelemetryChannel.EndpointAddress = "https://vortex.data.microsoft.com/collect/v1";
                TelemetryConfiguration.Active.InstrumentationKey = "AIF-a6c90b8a-c7f9-4d3f-96f4-d5f9dbd4b4ce";

                _telemetryClient = new TelemetryClient();
                _sessionId = Guid.NewGuid().ToString().ToUpperInvariant();
                
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.SystemIdentification")
                    && Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.System.Profile.SystemIdentification", "GetSystemIdForPublisher"))
                {
                    var systemId = SystemIdentification.GetSystemIdForPublisher();
                    if (systemId != null)
                    {
                        var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(systemId.Id);

                        byte[] bytes = new byte[systemId.Id.Length];
                        dataReader.ReadBytes(bytes);

                        _machineId = BitConverter.ToString(bytes);
                    }
                }                
            }
            catch (Exception)
            { }
        }

        public static void TrackEvent(string eventName)
        {
            if (_telemetryClient != null)
                TrackEvent(eventName, null, null);
        }

        public static void TrackEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            if (_telemetryClient != null)
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    properties.Add("SessionId", _sessionId);
                    properties.Add("MachineId", _machineId);
                }
                else
                {
                    if (!properties.ContainsKey("SessionId"))
                        properties.Add("SessionId", _sessionId);
                    if (!properties.ContainsKey("MachineId"))
                        properties.Add("MachineId", _machineId);
                }
                properties.Add("EventTime", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffzzz"));
                _telemetryClient.TrackEvent(eventName, properties, metrics);

                if (AutoFlushEvents)
                    _telemetryClient.Flush();
            }
        }

        public static void FlushEvents()
        {
            if (_telemetryClient != null)
                _telemetryClient.Flush();
        }
    }
}
