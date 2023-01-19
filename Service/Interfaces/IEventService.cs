using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Models;

namespace VRefSolutions.Service.Interfaces
{
    public interface IAltitudeService
    {
        Altitude CreateAltitude(Altitude newAltitude);
        List<Altitude> GetAltitudesByTrainingId(int trainingId);
        List<Altitude> GetAltitudesInRangeByTrainingId(int trainingId, TimeStamp time, int range);
    }
}