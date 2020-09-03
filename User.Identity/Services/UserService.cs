using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Options;
using User.Identity.Dtos;

namespace User.Identity.Services {
    public class UserService : IUserService {
        //readonly string _userServiceUrl = "http://localhost:8002";
        readonly string _userServiceUrl;
        private readonly HttpClient httpClient;

        public UserService (HttpClient httpClient, IDnsQuery dnsQuery, IOptions<ServiceDiscoveryOptions> serviceDisvoveryOptions) {
            this.httpClient = httpClient;

            var address = dnsQuery.ResolveService ("service.consul", serviceDisvoveryOptions.Value.UserServiceName);
            var addressList = address.First ().AddressList;
            var host = addressList.Any () ? addressList.First ().ToString () : address.First ().HostName;
            var port = address.First ().Port;

            _userServiceUrl = $"http://{host}:{port}";
        }
        public async Task<int> CheckOrCreate (string phone) {
            var form = new Dictionary<string, string> { { "phone", phone } };
            var content = new FormUrlEncodedContent (form);
            var response = await httpClient.PostAsync (_userServiceUrl + "/api/users/check-or-create", content);

            if (response.StatusCode == HttpStatusCode.OK) {
                var userId = await response.Content.ReadAsStringAsync ();
                int.TryParse (userId, out int intUserId);
                return intUserId;
            }
            return 0;
        }
    }
}