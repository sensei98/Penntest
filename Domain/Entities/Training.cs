using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Interfaces;

namespace VRefSolutions.Domain.Entities
{
    public class Training : IBaseEntity
    {
        
        public int Id { get; set; }

        public List<string>? Videos { get; set; } 
        
        public List<Event>? Events { get; set; }

        
        public List<Altitude>? Altitudes { get; set; }

        
        public List<User>? Participants { get; set; }
        
        public DateTime CreationDateTime { get; set; }
        
        public Status Status { get; set; }

        public Training(List<User> participants)
        {
            Participants = participants;
            CreationDateTime = DateTime.Now;
            Events = new List<Event>();
            Videos = new List<string>();
            Altitudes = new List<Altitude>();
            Status = Status.Created;
        }
        public Training(){}
    }
}