using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRefSolutions.Service.Interfaces
{
    public interface IEmailService
    {
        bool sendEmail(string emailAddress, string subject, string content);
        public bool IsValidEmail(string email);
    }
}
