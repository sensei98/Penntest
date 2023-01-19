using VRefSolutions.Domain.Interfaces;
using VRefSolutions.Domain.Models;

namespace VRefSolutions.Domain.Entities
{
    public class Altitude : IBaseEntity
    {
        public int Id { get; set; }
        public Training Training { get; set; }
        public double Amsl { get; set; }
        public TimeStamp TimeStamp { get; set; }

        public Altitude() { }
    }
}