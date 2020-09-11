using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
namespace Resilience {
    public interface IHttpClient {
        Task<HttpResponseMessage> PostAsync<T> (string url, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");
        /// <summary>
        /// 向指定API URI添加验证头指定请求方式,返回请求结果
        /// </summary>
        /// <param name="uri">请头API地址</param>
        /// <param name="authorizationToken">验证TOKEN</param>
        /// <param name="authorizationMethod">请求方式GET</param>
        /// <returns>响应数据</returns>
        Task<string> GetStringAsync (string uri, string authorizationToken = null, string authorizationMethod = "Bearer");
        Task<HttpResponseMessage> PutAsync<T> (string uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");
        Task<HttpResponseMessage> PostAsync (string url, Dictionary<string, string> form, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer");
    }
}