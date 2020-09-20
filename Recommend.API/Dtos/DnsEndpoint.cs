using System.Net;
namespace Recommend.API.Dtos {
    public class DnsEndpoint {
        public string Address { get; set; }
        public int Port { get; set; }
        public IPEndPoint ToIPEndPoint () {
            return new IPEndPoint (IPAddress.Parse (Address), Port);
        }
    }
}