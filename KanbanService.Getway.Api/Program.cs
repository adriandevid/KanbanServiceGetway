using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;
using Ocelot.Cache.CacheManager;
using KanbanService.Getway.Api.Models;
using Consul;
using ProjetoService.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IHostedService, ServiceRecoverConfig>();
builder.Services.Configure<GetwayConfiguration>(builder.Configuration.GetSection("GetwayService"));
builder.Services.Configure<ConsulConfiguration>(builder.Configuration.GetSection("Consul"));
builder.Services.AddOcelot(builder.Configuration).AddConsul().AddCacheManager(x => x.WithDictionaryHandle());

var consulAddress = builder.Configuration.GetSection("Consul")["Url"];

builder.Services.AddSingleton<IConsulClient, ConsulClient>(provider =>
    new ConsulClient(config => {
        config.Address = new Uri(consulAddress);
    }));




var app = builder.Build();

///<summary>
/// Validação para auto preservação
/// </summary>
app.MapGet("/health", () =>
{
    return "OK";
})
.WithName("health");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.Run();