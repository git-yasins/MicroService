
namespace Contact.API.Dtos
{
    public class ServiceDiscoveryOptions
    {
        public string UserServiceName { get; set; }
        public ConsulOptions Consul { get; set; }
    }
}