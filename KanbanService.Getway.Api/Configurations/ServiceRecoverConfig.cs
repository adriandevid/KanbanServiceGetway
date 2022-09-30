using Consul;
using KanbanService.Getway.Api.Models;
using Microsoft.Extensions.Options;

namespace ProjetoService.Configurations
{
    public class ServiceRecoverConfig : IHostedService
    {
        private IConsulClient _consulClient;
        private ConsulConfiguration _consulConfiguration;
        private GetwayConfiguration _projetoConfiguration;
        private ILogger<ServiceRecoverConfig> _logger;

        public ServiceRecoverConfig(IConsulClient consulClient,
            IOptions<GetwayConfiguration> projetoConfiguration,
            IOptions<ConsulConfiguration> consulConfiguration,
            ILogger<ServiceRecoverConfig> logger)
        {
            _consulClient = consulClient;
            _consulConfiguration = consulConfiguration.Value;
            _projetoConfiguration = projetoConfiguration.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var menuUri = new Uri(_projetoConfiguration.Url);
            
            var headerProperties = new Dictionary<string, List<string>>();
            headerProperties.Add("Content-Type", new List<string> { "application/json" });

            var serviceRegistration = new AgentServiceRegistration()
            {
                Address = menuUri.Host,
                Name = _projetoConfiguration.ServiceName,
                Port = menuUri.Port,
                ID = _projetoConfiguration.ServiceId,
                Tags = new[] { _projetoConfiguration.ServiceName },
                Check = new AgentServiceCheck()
                {
                    Name = "Validação de saúde do Serviço!",
                    HTTP = $"{_projetoConfiguration.Url}health",
                    TLSSkipVerify = false,
                    Method = "GET",
                    Header = headerProperties,
                    Interval = TimeSpan.FromSeconds(5)
                }
            };

            await _consulClient.Agent.ServiceDeregister(_projetoConfiguration.ServiceId, cancellationToken);
            await _consulClient.Agent.ServiceRegister(serviceRegistration, cancellationToken);

            await _consulClient.Agent.CheckRegister(new AgentCheckRegistration
            {
                ServiceID = _projetoConfiguration.ServiceId,
                Name = _projetoConfiguration.ServiceId,
                HTTP = $"{_projetoConfiguration.Url}health",
                TLSSkipVerify = false,
                Method = "GET",
                Header = headerProperties,
                Interval = TimeSpan.FromSeconds(5)
            });

            await _consulClient.Agent.Checks(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _consulClient.Agent.ServiceDeregister(_projetoConfiguration.ServiceId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when trying to de-register", ex);
            }
        }
    }
}
