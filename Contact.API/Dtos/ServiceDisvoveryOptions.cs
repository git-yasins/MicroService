namespace Contact.API.Dtos {
    public class ServiceDiscoveryOptions {
        public string UserServiceName { get; set; }
        public string ContactServiceName { get; set; }
        public ConsulOptions Consul { get; set; }
    }
}