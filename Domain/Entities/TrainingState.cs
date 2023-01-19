using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using VRefSolutions.Domain.Interfaces;
using VRefSolutions.Domain.Models;

namespace VRefSolutions.Domain.Entities
{
    public class TrainingState : IBaseEntity
    {
        public int Id { get; set; }
        public Training Training { get; set; }
        public int Altitude { get; set; }
        public string[] EcamMessages { get; set; }

        public TrainingState() { }
    }
}