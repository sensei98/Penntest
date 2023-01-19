using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VRefSolutions.Service.Interfaces;

namespace Service
{
    public class JwtMiddleware : IFunctionsWorkerMiddleware
    {
        ITokenService TokenService { get; }
        ILogger Logger { get; }
        IUserService UserService {get;}

        public JwtMiddleware(ITokenService TokenService, ILogger<JwtMiddleware> Logger, IUserService userService)
        {
            this.TokenService = TokenService;
            this.Logger = Logger;
            UserService = userService;
        }

        public async Task Invoke(FunctionContext Context, FunctionExecutionDelegate Next)
        {
            string functionName = Context.FunctionDefinition.Name;
            Logger.LogInformation(functionName);

            if (!functionName.Equals("TrainingTimerTrigger") && !functionName.Equals("predictAltitude") && !functionName.Equals("predictEcamEvents") && !functionName.Equals("predictInstruments"))
            {
                string HeadersString = (string)Context.BindingContext.BindingData["Headers"];

                Dictionary<string, string> Headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(HeadersString);

                if (Headers.TryGetValue("Authorization", out string AuthorizationHeader))
                {
                    try
                    {
                        AuthenticationHeaderValue BearerHeader = AuthenticationHeaderValue.Parse(AuthorizationHeader);

                        ClaimsPrincipal User = await TokenService.GetByValue(BearerHeader.Parameter);
                        int userId =int.Parse(User.Claims.Where(c => c.Type == ClaimTypes.PrimarySid).First().Value);
                        if(!UserService.GetUserById(userId).Equals(null))
                            Context.Items["User"] = User;
                        
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e.Message);
                    }
                }
            }
            await Next(Context);
        }

    }
}
