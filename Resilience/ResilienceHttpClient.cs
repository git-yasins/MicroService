using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;

namespace Resilience {
    public class ResilienceHttpClient : IHttpClient {
        HttpClient _httpClient;
        //根据Url origin去创建 policy
        readonly Func<string, IEnumerable<Policy>> _policyCreator;
        //将policy打包成组合policy wraper,进行本地缓存
        readonly ConcurrentDictionary<string, PolicyWrap> _policyWrappers;
        ILogger<ResilienceHttpClient> _logger;
        IHttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient (Func<string, IEnumerable<Policy>> policyCreator, ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor) {
            _httpClient = new HttpClient ();
            _policyWrappers = new ConcurrentDictionary<string, PolicyWrap> ();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _policyCreator = policyCreator;
        }

        public Task<HttpResponseMessage> PostAsync<T> (string url, T item, string authorizationToken, string requestId = null, string authorizationMethod = "Bearer") {
            Func<HttpRequestMessage> func = () => { return new HttpRequestMessage (HttpMethod.Post, url) { Content = new StringContent (JsonConvert.SerializeObject (item), Encoding.UTF8, "application/json") }; };
            return DoPostPutAsync (HttpMethod.Post, url, func, authorizationToken, requestId, authorizationMethod);
        }

        public Task<HttpResponseMessage> PostAsync (string url, Dictionary<string, string> form, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer") {
            Func<HttpRequestMessage> func = () => { return new HttpRequestMessage (HttpMethod.Post, url) { Content = new FormUrlEncodedContent (form) }; };
            return DoPostPutAsync (HttpMethod.Post, url, func, authorizationToken, requestId, authorizationMethod);
        }

        public Task<HttpResponseMessage> PutAsync<T> (string uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer") {
            Func<HttpRequestMessage> func = () => { return new HttpRequestMessage (HttpMethod.Post, uri) { Content = new StringContent (JsonConvert.SerializeObject (item), Encoding.UTF8, "application/json") }; };
            return DoPostPutAsync (HttpMethod.Put, uri, func, authorizationToken, requestId, authorizationMethod);
        }

        /// <summary>
        /// 公共POST|PUT执行方法
        /// </summary>
        /// <param name="method">post|put</param>
        /// <param name="url">api路径</param>
        /// <param name="requestMessageAction">请求对象 添加jwt Token Authorization Headers</param>
        /// <param name="authorizationToken">请求TOKEN</param>
        /// <param name="requestId">x-requestid</param>
        /// <param name="authorizationMethod">Bearer方式</param>
        /// <returns>响应数据</returns>
        private Task<HttpResponseMessage> DoPostPutAsync (
            HttpMethod method,
            string url,
            Func<HttpRequestMessage> requestMessageAction,
            string authorizationToken = null,
            string requestId = null,
            string authorizationMethod = "Bearer") {

            if (method != HttpMethod.Post && method != HttpMethod.Put) {
                throw new ArgumentException ("Value must be either post or put.", nameof (method));
            }

            var origin = GetOriginFromUri (url);

            return HttpInvoker (origin, async () => {
                HttpRequestMessage requestMessage = requestMessageAction ();
                SetAuthorizationHeader (requestMessage);

                if (authorizationToken != null) {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue (authorizationMethod, authorizationToken);
                }

                if (requestId != null) {
                    requestMessage.Headers.Add ("x-requestid", requestId);
                }

                var response = await _httpClient.SendAsync (requestMessage);
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) {
                    throw new HttpRequestException ();
                }
                return response;
            });
        }

        private void SetAuthorizationHeader (HttpRequestMessage requestMessage) {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty (authorizationHeader)) {
                requestMessage.Headers.Add ("Authorization", new List<string> { authorizationHeader });
            }
        }

        private async Task<T> HttpInvoker<T> (string origin, Func<Task<T>> action) {
            var normalizedOrigin = NormalizedOrigin (origin);
            if (!_policyWrappers.TryGetValue (normalizedOrigin, out PolicyWrap policyWrap)) {
                policyWrap = Policy.WrapAsync (_policyCreator (normalizedOrigin).ToArray ());
                _policyWrappers.TryAdd (normalizedOrigin, policyWrap);
            }

            return await policyWrap.ExecuteAsync (action, new Context (normalizedOrigin));
        }

        private string NormalizedOrigin (string origin) {
            return origin?.Trim ()?.ToLower ();
        }

        private static string GetOriginFromUri (string uri) {
            var url = new Uri (uri);
            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";
            return origin;
        }
        /// <summary>
        /// 向指定API URI添加验证头指定请求方式,返回请求结果
        /// </summary>
        /// <param name="uri">请头API地址</param>
        /// <param name="authorizationToken">验证TOKEN</param>
        /// <param name="authorizationMethod">请求方式GET</param>
        /// <returns></returns>
        public Task<string> GetStringAsync (string uri, string authorizationToken = null, string authorizationMethod = "Bearer") {
            var origin = GetOriginFromUri (uri);
            return HttpInvoker (origin, async () => {
                var requestMessage = new HttpRequestMessage (HttpMethod.Get, uri);

                SetAuthorizationHeader (requestMessage);
                if (authorizationToken != null) {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue (authorizationMethod, authorizationToken);
                }

                var response = await _httpClient.SendAsync (requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError) {
                    throw new HttpRequestException ();
                }

                if (!response.IsSuccessStatusCode) {
                    return null;
                }

                return await response.Content.ReadAsStringAsync ();
            });
        }
    }
}