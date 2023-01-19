using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class FunctionContextExtension
    {
        public static ClaimsPrincipal GetUser(this FunctionContext Context)
        {
            if (Context.Items.TryGetValue("User", out object User))
            {
                return (ClaimsPrincipal)User; ;
            }
            else return null;
        }
    }
}
