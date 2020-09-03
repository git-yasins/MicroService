namespace User.Identity.Dtos
{
    public class ServiceDiscoveryOptions
    {
        public string UserServiceName { get; set; }
        public ConsulOptions Consul { get; set; }
    }
}