namespace Recommend.API.Dtos {
    public class ServiceDiscoveryOptions {
        public string ContactServiceName { get; set; }
        public ConsulOptions Consul { get; set; }
        public string UserServiceName { get; set;}
    }
}