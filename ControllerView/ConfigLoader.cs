using Newtonsoft.Json;

namespace ControllerView
{
    internal struct ConfigLoader
    {
        [JsonProperty("Nitro")]
        public Button Nitro { get; private set; }

        [JsonProperty("Handbrake")]
        public Button Handbrake { get; private set; }

        [JsonProperty("GearUp")]
        public Button GearUp { get; private set; }

        [JsonProperty("GearDown")]
        public Button GearDown { get; private set; }

        [JsonProperty("BoT")]
        public Button BoT { get; private set; }

        [JsonProperty("Colour")]
        public Colours Colours { get; private set; }
    }

    internal struct Button
    {
        [JsonProperty("Colour")]
        public string ColourHex { get; private set; }

        [JsonProperty("Button")]
        public ushort ButtonNumber { get; private set; }

        [JsonProperty("Button2")]
        public ushort ComboButtonNumber { get; private set; }
    }

    internal struct Colours
    {
        [JsonProperty("Left")]
        public string LeftHex { get; private set; }

        [JsonProperty("Right")]
        public string RightHex { get; private set; }

        [JsonProperty("Accel")]
        public string AccelHex { get; private set; }

        [JsonProperty("Decel")]
        public string DecelHex { get; private set; }

        [JsonProperty("Background")]
        public string BackgroundHex { get; private set; }

        [JsonProperty("Borders")]
        public bool HaveBorders { get; private set; }
    }
}