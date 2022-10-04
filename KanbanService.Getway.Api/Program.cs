using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;
using Ocelot.Cache.CacheManager;
using KanbanService.Getway.Api.Models;
using Consul;
using ProjetoService.Configurations;
using Ocelot.Middleware;
using Microsoft.OpenApi.Models;
using MMLib.Ocelot.Provider.AppConfiguration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddSingleton<IHostedService, ServiceRecoverConfig>();
builder.Services.Configure<GetwayConfiguration>(builder.Configuration.GetSection("GetwayService"));
builder.Services.Configure<ConsulConfiguration>(builder.Configuration.GetSection("Consul"));

var consulAddress = builder.Configuration.GetSection("Consul")["Url"];

builder.Services.AddSingleton<IConsulClient, ConsulClient>(provider =>
    new ConsulClient(config =>
    {
        config.Address = new Uri(consulAddress);
    }));

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOcelot(builder.Configuration).AddAppConfiguration().AddConsul().AddCacheManager(x => x.WithDictionaryHandle());
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

/////<summary>
///// Validação para auto preservação
///// </summary>
//app.MapGet("/health", () =>
//{
//    return "OK";
//})
//.WithName("health");

app.UseSwagger();
app.UseStaticFiles();
app.UseSwaggerForOcelotUI()
.UseOcelot()
.Wait();


//app.UseHttpsRedirection();
app.Run();