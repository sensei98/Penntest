using VRefSolutions.Domain.Entities;

namespace VRefSolutions.Repository.Interfaces
{
    public interface ITrainingStateRepository : IBaseRepository<TrainingState>
    {
        public TrainingState? GetByTrainingId(int trainingId);
    }
}