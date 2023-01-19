using VRefSolutions.Domain.Models;

namespace VRefSolutions.Service.Interfaces
{
    public interface IPredictionService
    {
        public Task PredictInstruments(int trainingId, TimeStamp timestamp, byte[] image);
        public Task PredictEcamEvents(PositionMessage positionMessage);
        public Task PredictAltitude(PositionMessage positionMessage);
    }
}