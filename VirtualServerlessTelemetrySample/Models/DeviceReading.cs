using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualServerlessTelemetrySample.Models
{
    public class DeviceReading
    {
        [JsonProperty(PropertyName = "id")]
        public string DeviceId { get; set; }
        [JsonProperty(PropertyName = "temperature")]
        public decimal DeviceTemperature { get; set; }
        [JsonProperty(PropertyName = "damageLevel")]
        public string DamageLevel { get; set; }
        [JsonProperty(PropertyName = "ageInDays")]
        public int DeviceAgeInDays { get; set; }
        [JsonProperty(PropertyName = "location")]
        public string DeviceLocation { get; set; }
    }
}
