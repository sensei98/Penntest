using VRefSolutions.Domain.Interfaces;

namespace VRefSolutions.Domain.Entities
{
    public class EventType : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string Symbol { get; set; }
        //public List<Event> Events {get;set;}
    }
}