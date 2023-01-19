using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using VRefSolutions.Domain.Enums;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class CameraDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "Name of what the camera is capturing.")]
        public string Name { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The RTMP or RSTP stream URL which the server needs to connect to the cameras.")]
        public string Url { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The Username required for authenticating to the camera stream.")]
        public string Username { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The Password required for authenticating to the camera stream.")]
        public string Password { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Determines whether to record Video, Audio, or Both")]
        public CaptureMode CaptureMode { get; set; }
    }
}
