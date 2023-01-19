using BoundingBox = Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.BoundingBox;

namespace VRefSolutions.Domain.Models
{
    public class PositionMessage
    {
        public BoundingBox Position { get; set; }
        public int TrainingId { get; set; }
        public TimeStamp Timestamp { get; set; }
        public string SnapshotBlobName { get; set; }

        public PositionMessage(BoundingBox position, int trainingId, TimeStamp timestamp, string snapshotBlob)
        {
            Position = position;
            TrainingId = trainingId;
            Timestamp = timestamp;
            SnapshotBlobName = snapshotBlob;
        }
    }
}