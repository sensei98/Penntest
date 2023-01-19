using VRefSolutions.Domain.Interfaces;
using VRefSolutions.Domain.Models;
namespace VRefSolutions.Domain.Entities
{
    public class Event : IBaseEntity
    {

        public int Id { get; set; }
        public Training Training { get; set; }
        public EventType EventType {get;set;}
        public string OverwriteName { get; set; }
        public string OverwriteMessage {get;set;}
        public string OverwriteSymbol { get; set; }
        public TimeStamp TimeStamp { get; set; }

        public Event()
        {
        }
    }
}