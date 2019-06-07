using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsiderDevTour.Sensors.Constants
{
    /// <summary>
    /// the external service credentials we'll be using.
    /// </summary>
    public static class AzureCredentials
    {
        // Face API subscription key and group
        public const string subscriptionKey = "";
        public const string serviceEndpoint = "";
        public const string personGroup = "";
        // IoT Central IoT Hub credentials
        public const string hubDeviceId = "";
        public const string hubConnectionString = "";

        public const string centralDeviceId = "";
        public const string centralConnectionString = "";

        public const string AccountName = "";
        public const string AccountKey = "";
        public const string ImageContainer = "";
    }
}

/*

[Device] Creating Device 'RaspberryPi3B'
[Device] [Create][Success] status: 200 OK
[Device] [Create] device info: {
  "deviceId": "RaspberryPi3B",
  "generationId": "636952022448893208",
  "etag": "OTMzMzg1ODE2",
  "connectionState": "Disconnected",
  "status": "enabled",
  "statusReason": null,
  "connectionStateUpdatedTime": "0001-01-01T00:00:00",
  "statusUpdatedTime": "0001-01-01T00:00:00",
  "lastActivityTime": "0001-01-01T00:00:00",
  "cloudToDeviceMessageCount": 0,
  "capabilities": {
    "iotEdge": false
  },
  "authentication": {
    "symmetricKey": {
      "primaryKey": "mWO5BwVcGazLPWaM72g972iq7Yqk06qHMS+RRg1Z4Ss=",
      "secondaryKey": "EFQYVAecoYqbkit+/O0kHh8nxtAJg8KgrTTvp/N2zS0="
    },
    "x509Thumbprint": {
      "primaryThumbprint": null,
      "secondaryThumbprint": null
    },
    "type": "sas",
    "SymmetricKey": {
      "primaryKey": "mWO5BwVcGazLPWaM72g972iq7Yqk06qHMS+RRg1Z4Ss=",
      "secondaryKey": "EFQYVAecoYqbkit+/O0kHh8nxtAJg8KgrTTvp/N2zS0="
    }
  },
  "connectionString": "HostName=InsiderDevTourMed.azure-devices.net;DeviceId=RaspberryPi3B;SharedAccessKey=mWO5BwVcGazLPWaM72g972iq7Yqk06qHMS+RRg1Z4Ss="
}

 */
