using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service.Interfaces;

namespace VRefSolutions.Service
{
    public class AltitudeService : IAltitudeService
    {
        private IAltitudeRepository AltitudeRepository;

        public AltitudeService(IAltitudeRepository altitudeRepository)
        {
            AltitudeRepository = altitudeRepository;
        }

        public Altitude CreateAltitude(Altitude newAltitude)
        {
            return AltitudeRepository.Add(newAltitude);
        }

        public List<Altitude> GetAltitudesByTrainingId(int trainingId)
        {
            return AltitudeRepository.GetAltitudesByTrainingId(trainingId);
        }

        public List<Altitude> GetAltitudesInRangeByTrainingId(int trainingId, TimeStamp time, int range)
        {
            return AltitudeRepository.GetAltitudesByTrainingId(trainingId).Where(e => e.TimeStamp.IsWithinRange(time, range)).ToList();
        }
    }

}