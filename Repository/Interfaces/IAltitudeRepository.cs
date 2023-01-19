using VRefSolutions.Domain.Entities;

namespace VRefSolutions.Repository.Interfaces
{
    public interface IAltitudeRepository : IBaseRepository<Altitude>
    {
        List<Altitude> GetAltitudesByTrainingId(int trainingId);
    }
}