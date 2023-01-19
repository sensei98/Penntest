using VRefSolutions.Domain.Interfaces;

namespace VRefSolutions.Domain.Entities
{
    public class EcamMessage : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsAccepted { get; set; }

        public EcamMessage()
        {
        }
    }
}