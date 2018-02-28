using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace CompositionSampleGallery.Shared
{
    internal static class AppTelemetryClient
    {
        private static TelemetryClient _telemetryClient = null;
        private static string _sessionId = null;
        private static string _machineId = Guid.Empty.ToString();
        private static bool _isInitialized = false;

        private struct AppInsightsEvent
        {
            public string Name;
            public Dictionary<string, string> Properties;
            public Dictionary<string, double> Metrics;

            public AppInsightsEvent(string eventName, Dictionary<string, string> eventProperties, Dictionary<string, double> eventMetrics)
            {
                Name = eventName;
                Properties = eventProperties;
                Metrics = eventMetrics;
            }
        };
        private static Queue<AppInsightsEvent> _eventQueue = new Queue<AppInsightsEvent>();

        static AppTelemetryClient()
        {
            InitializeAppTelemetryClientAsync();
        }

        private static Task InitializeAppTelemetryClientAsync()
        {
            Task initializeClient = new Task(() =>
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

                _isInitialized = true;
            });

            initializeClient.Start();
            return initializeClient;
        }

        public static void TrackEvent(string eventName)
        {
            _eventQueue.Enqueue(new AppInsightsEvent(eventName, null, null));
        }

        public static void TrackEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            _eventQueue.Enqueue(new AppInsightsEvent(eventName, properties, metrics));
        }

        public static void FlushEvents()
        {
            if (_telemetryClient != null && _isInitialized)
            {
                for (AppInsightsEvent queuedEvent; _eventQueue.Count > 0;)
                {
                    queuedEvent = _eventQueue.Dequeue();

                    if (queuedEvent.Properties == null)
                    {
                        queuedEvent.Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        queuedEvent.Properties.Add("SessionId", _sessionId);
                        queuedEvent.Properties.Add("MachineId", _machineId);
                    }
                    else
                    {
                        if (!queuedEvent.Properties.ContainsKey("SessionId"))
                            queuedEvent.Properties.Add("SessionId", _sessionId);
                        if (!queuedEvent.Properties.ContainsKey("MachineId"))
                            queuedEvent.Properties.Add("MachineId", _machineId);
                    }

                    queuedEvent.Properties.Add("EventTime", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffzzz"));
                    _telemetryClient.TrackEvent(queuedEvent.Name, queuedEvent.Properties, queuedEvent.Metrics);

                    _telemetryClient.Flush();
                }
            }
        }
    }
}
