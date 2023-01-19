using System.Collections.Generic;
using VRefSolutions.Domain.Interfaces;

namespace VRefSolutions.Domain.Entities
{
    public class Organization : IBaseEntity
    {
        public int Id { get; set; }        
        public string? Name { get; set; }
        public List<User>? Users { get; set; }
    }
}