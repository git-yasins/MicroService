using System;
using System.Linq;
using System.Threading.Tasks;
using Recommend.API.Dtos;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience;
namespace Recommend.API.Services
{
    public class UserService : IUserService {
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
        /// <summary>
        /// 根据用户ID查询用户基本信息,用于发起添加好友请求
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task<UserIdentity> GetBaseUserInfoAsync (int userId) {
            _logger.LogTrace ($"Enter into GetBaseUserInfoAsync:{userId}");
            try {
                var response = await httpClient.GetStringAsync (_userServiceUrl + $"/api/users/baseinfo/{userId}");
                if (!string.IsNullOrEmpty (response)) {
                    UserIdentity userInfo = JsonConvert.DeserializeObject<UserIdentity> (response);

                    _logger.LogTrace ($"Completed GetBaseUserInfoAsync with userId:{userInfo.UserId}");
                    return userInfo;
                }
            } catch (Exception ex) {
                _logger.LogError ("GetBaseUserInfoAsync 在重试之后失败," + ex.Message + ex.StackTrace);
                throw ex;
            }
            return null;
        }
    }
}