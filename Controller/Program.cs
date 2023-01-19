using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VRefSolutions.DAL;
using Microsoft.EntityFrameworkCore;
using VRefSolutions.Service.Interfaces;
using VRefSolutions.Service;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Repository;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.DTO;
using Service;
using VRefSoltutions.Profiles;

string? connectionString = Environment.GetEnvironmentVariable("connectionString");
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(Worker => Worker.UseNewtonsoftJson().UseMiddleware<JwtMiddleware>())
    .ConfigureServices(services =>
    {
        services.Configure<JsonSerializerSettings>(options =>
                    {
                        options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    });
        services.AddDbContext<VRefSolutionsContext>(
            options =>
            {
                options.UseSqlServer(connectionString); 
                options.EnableSensitiveDataLogging();
            });
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
        services.AddSingleton<IAltitudeService, AltitudeService>();
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IEventTypeService, EventTypeService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IPredictionService, PredictionService>();
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ITrainingRepository, TrainingRepository>();
        services.AddScoped<ITrainingStateRepository, TrainingStateRepository>();
        services.AddScoped<IAltitudeRepository, AltitudeRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventTypeRepository, EventTypeRepository>();
        services.AddScoped<IEcamMessageRepository, EcamMessageRepository>();
    })
    .ConfigureOpenApi()

    .Build();

host.Run();

