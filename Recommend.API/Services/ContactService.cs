using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Recommend.API.Dtos;
using Resilience;

namespace Recommend.API.Services {
    public class ContactService : IContactService {
        readonly string _contactServiceUrl;
        private readonly IHttpClient httpClient;
        private readonly ILogger<ResilienceHttpClient> _logger;

        public ContactService (IHttpClient httpClient, IDnsQuery dnsQuery, IOptions<ServiceDiscoveryOptions> serviceDisvoveryOptions, ILogger<ResilienceHttpClient> logger) {
            this.httpClient = httpClient;
            this._logger = logger;
            var address = dnsQuery.ResolveService ("service.consul", serviceDisvoveryOptions.Value.ContactServiceName);
            var addressList = address.First ().AddressList;
            var host = addressList.Any () ? addressList.First ().ToString () : address.First ().HostName;
            var port = address.First ().Port;

            _contactServiceUrl = $"http://{host}:{port}";
        }
        public async Task<List<Contact>> GetContactsByUserId (int userId) {
            _logger.LogTrace ($"Enter into GetContactsByUserId:{userId}");
            try {
                var response = await httpClient.GetStringAsync (_contactServiceUrl + $"/api/contacts/{userId}");
                if (!string.IsNullOrEmpty (response)) {
                    List<Contact> contactInfo = JsonConvert.DeserializeObject<List<Contact>> (response);

                    _logger.LogTrace ($"Completed GetContactsByUserId with userId:{userId}");
                    return contactInfo;
                }
            } catch (Exception ex) {
                _logger.LogError ("GetContactsByUserId 在重试之后失败," + ex.Message + ex.StackTrace);
                throw ex;
            }
            return null;
        }
    }
}