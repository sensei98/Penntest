using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VRefSolutions.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Status
    {
        Created,
        Recording,
        Paused,
        Processing,
        Finished
    }
}