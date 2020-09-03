using System.Collections;
using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
namespace IdentityServerCenter {
    public class Config {
        public static IEnumerable<ApiResource> GetApiResource () {
            return new List<ApiResource> { new ApiResource ("getway_api", "user service") };
        }

        public static IEnumerable<Client> GetClients () {
            return new List<Client> {
                new Client () {
                    ClientId = "android", //客户端模式
                        ClientSecrets = new List<Secret> { new Secret ("secret".Sha256 ()) },
                        RefreshTokenExpiration = TokenExpiration.Sliding,
                        AllowOfflineAccess = true,
                        RequireClientSecret = false, //client_secret非必传
                        AllowedGrantTypes = new List<string> { "sms_auth_code" },
                        AlwaysIncludeUserClaimsInIdToken = true,
                        AllowedScopes = new List<string> {
                        "getway_api",
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                        }
                        }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources () {
            return new List<IdentityResource> {
                new IdentityResources.OpenId (),
                new IdentityResources.Profile ()
            };
        }
    }
}