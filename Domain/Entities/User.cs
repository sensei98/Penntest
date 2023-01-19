using System;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Interfaces;

namespace VRefSolutions.Domain.Entities
{
    public class User : IBaseEntity
    {
        
        public int Id { get;set; }

        
        public string Email { get;set; }

        
        public string? Password {get;set;} // hashed

        
        public string? FirstName { get; set; }

        
        public string? LastName { get; set; }

        
        public Role UserType { get; set; }

        
        public Organization Organization { get; set; }

        
        public string? ActivationCode { get; set; }

        public List<Training> Trainings {get;set;}

    }
}