using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience;
using User.Identity.Dto;
using User.Identity.Dtos;

namespace User.Identity.Services {
    public class UserService : IUserService {
        //readonly string _userServiceUrl = "http://localhost:8002";
        readonly string _userServiceUrl;
        private readonly IHttpClient httpClient;
        private readonly ILogger<ResilienceHttpClient> _logger;

        public UserService (IHttpClient httpClient, IDnsQuery dnsQuery, IOptions<ServiceDiscoveryOptions> serviceDisvoveryOptions, ILogger<ResilienceHttpClient> logger) {
            this.httpClient = httpClient;
            this._logger = logger;
            var address = dnsQuery.ResolveService ("service.consul", serviceDisvoveryOptions.Value.UserServiceName);
            var addressList = address.First ().AddressList;
            var host = addressList.Any () ? addressList.First ().ToString () : address.First ().HostName;
            var port = address.First ().Port;

            _userServiceUrl = $"http://{host}:{port}";
        }

        public async Task<UserInfo> CheckOrCreate (string phone) {
            var form = new Dictionary<string, string> { { "phone", phone } };
            var content = new FormUrlEncodedContent (form);
            try {
                var response = await httpClient.PostAsync (_userServiceUrl + "/api/users/check-or-create", form);
                if (response.StatusCode == HttpStatusCode.OK) {
                    var result = await response.Content.ReadAsStringAsync ();
                    UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo> (result);

                    _logger.LogTrace ($"Completed CheckOrCreate with userId:{userInfo.Id}");
                    return userInfo;
                }
            } catch (Exception ex) {
                _logger.LogError ("CheckOrCreate 在重试之后失败," + ex.Message + ex.StackTrace);
                throw ex;
            }
            return null;
        }
    }
}