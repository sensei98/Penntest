using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VRefSolutions.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CaptureMode
    {  
        Video,
        Audio,
        Video_Audio
    }
}