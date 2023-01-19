using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Service;
using VRefSoltutions.Profiles;
using VRefSolutions.DAL;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service;
using VRefSolutions.Service.Interfaces;

namespace TestUnit
{
    public class ControllerTests
    {
        protected IHost host { get; }
        public ControllerTests()
        {
            host = new HostBuilder()
            .ConfigureWebJobs()
            .ConfigureFunctionsWorkerDefaults(Worker => Worker.UseNewtonsoftJson().UseMiddleware<JwtMiddleware>())
            .ConfigureServices(services =>
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder
                { DataSource = ":memory:" };
                var connectionString = connectionStringBuilder.ToString();

                //This creates a SqliteConnectionwith that string
                var connection = new SqliteConnection(connectionString);

                //The connection MUST be opened here
                connection.Open();
                services.Configure<JsonSerializerSettings>(options =>
                            {
                                options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                            });
                services.AddDbContext<VRefSolutionsContext>(
                    options => options.UseSqlite(connection));
                services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<UserCreateDTO, User>().ReverseMap();
                cfg.CreateMap<User, UserResponseDTO>().ReverseMap();
                cfg.CreateMap<User, UserUpdateDTO>().ReverseMap();
                cfg.CreateMap<OrganizationDTO, Organization>().ReverseMap();
                cfg.CreateMap<OrganizationResponseDTO, Organization>().ReverseMap();
                cfg.CreateMap<OrganizationsDTO, Organization>().ReverseMap();

                cfg.CreateMap<AltitudeResponseDTO, Altitude>().ReverseMap();
                cfg.AddProfile<EventProfile>();
                cfg.AddProfile<TrainingProfile>();
            });
                // Services
                services.AddSingleton<IUserService, UserService>();
                services.AddSingleton<IOrganizationService, OrganizationService>();
                services.AddSingleton<ITrainingService, TrainingService>();
                services.AddSingleton<IEventService, EventService>();
                services.AddSingleton<IEventTypeService, EventTypeService>();
                services.AddSingleton<ITokenService, TokenService>();
                services.AddSingleton<IEmailService, EmailService>();
                services.AddScoped<IPredictionService, PredictionService>();
                services.AddScoped<IAltitudeService, AltitudeService>();
                //services.AddSingleton<ILogger, Logger<TrainingController>>();
                // Repositories
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IOrganizationRepository, OrganizationRepository>();
                services.AddScoped<ITrainingRepository, TrainingRepository>();
                services.AddScoped<ITrainingStateRepository, TrainingStateRepository>();
                services.AddScoped<IEventRepository, EventRepository>();
                services.AddScoped<IEventTypeRepository, EventTypeRepository>();
                services.AddScoped<IEcamMessageRepository, EcamMessageRepository>();
                services.AddScoped<IAltitudeRepository, AltitudeRepository>();
            })
            .Build();
        }
        protected T GetDeserializedJsonObject<T>(string json) where T : class
        {
            // simple deserializer method so we can handle faulty json objects in the Validator.  
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return null;
            }
        }

        protected T GetDeserializedJsonObject<T>(Stream body) where T : class
        {
            return GetDeserializedJsonObject<T>(new StreamReader(body).ReadToEnd());
        }
    }
}