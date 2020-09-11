using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;

namespace Contact.API.Infrastructure {
    /// <summary>
    /// 弹性恢复客户端工厂
    /// </summary>
    public class ResilienceClientFactory {
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;
        //重试次数
        int _retryCount;
        //融断之前允许的异常次数
        int _exceptionCountAllowedBeforeBreaking;

        public ResilienceClientFactory (ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor, int retryCount, int exceptionCountAllowedBeforeBreaking) {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _retryCount = retryCount;
            _exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
        }
        public ResilienceHttpClient GetResilienceHttpClient() => new ResilienceHttpClient (origin => CreatePolicy (origin), _logger, _httpContextAccessor);

        private Policy[] CreatePolicy (string origin) {
 
            return new Policy[] {
                //等待重试
                Policy.Handle<HttpRequestException> ()
                    .WaitAndRetryAsync (_retryCount, retryAttempt => TimeSpan.FromSeconds (Math.Pow (2, retryAttempt)), (exception, TimeSpan, retryCount, context) => {
                        var msg = $"第 {retryCount} 次重试 " +
                            $"of {context.PolicyKey}, " +
                            $"at {context.ExecutionKey}, " +
                            $"due to: {exception}. ";
                        _logger.LogWarning (msg);
                        _logger.LogDebug (msg);
                    }),
                    //融断
                    Policy.Handle<HttpRequestException> ().CircuitBreakerAsync (_exceptionCountAllowedBeforeBreaking, TimeSpan.FromMinutes (1), (exception, duration) => {
                        //打开断路器
                        _logger.LogTrace ("熔断器打开");
                    }, () => {
                        _logger.LogTrace ("熔断器关闭");
                    })
            };
        }
    }
}