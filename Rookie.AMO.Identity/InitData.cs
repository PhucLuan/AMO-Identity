using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace Rookie.AMO.Identity
{
    public static class InitData
    {
        // test users

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "35d08332-a3dc-4e5b-8a35-ffe522ab3d61",
                    Username = "User1",
                    Password = "u123",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Jhon"),
                        new Claim("family_name", "Doe"),
                        new Claim("role", "Admin")
                    }
                },
                new TestUser
                {
                    SubjectId = "979e8d43-7f7d-4a1d-8c2d-59f145c0bfa1",
                    Username = "User2",
                    Password = "u456",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Jane"),
                        new Claim("family_name", "Dae"),
                        new Claim("role", "User")
                    }
                }
            };
        }

        // identity-related resources (scopes)
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResource(
                    name: "profile",
                    userClaims: new[] { "name", "website", "role" },
                    displayName: "Your profile data"
                ),
            };
        }
        public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("api1", "API #1") {Scopes = {"roles"} },
            // local API
            new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
        };

        public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("roles", new List<string>() { "role", "location" }),
        };

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>()
            {
                new Client
                {
                    ClientName = "Rookie.AMO",
                    ClientId = "rookieamoclient",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:5001/callback",
                        "https://b3g1-amo-id4.azurewebsites.net/callback"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:5001/",
                        "https://b3g1-amo-id4.azurewebsites.net/"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles"
                    },
                    ClientSecrets =
                    {
                        new Secret("rookieamosecret".Sha256())
                    },
                    //AllowedCorsOrigins = new List<string>
                    //{
                    //    "https://localhost:5001/"
                    //},
                    AllowAccessTokensViaBrowser = true
                },
                //swagger client from web api
                new Client
                {
                    ClientId = "demo_api_swagger",
                    ClientName = "Swagger UI for demo_api",
                    ClientSecrets = {new Secret("BCF15D0F-1EE5-4DDF-91FC-73B6E812B9AF".Sha256())}, // change me!

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris = {
                        "https://localhost:5011/swagger/oauth2-redirect.html",
                        "https://b3g1-amo-api.azurewebsites.net/swagger/oauth2-redirect.html"
                    },
                    AllowedCorsOrigins = {
                        "https://localhost:5011",
                        "https://b3g1-amo-api.azurewebsites.net"
                    },
                    AllowedScopes = {"roles" }
                },
                new Client
                {
                    ClientName = "Rookie.AMO.WebApi",
                    ClientId = "rookieamo",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:5011/callback",
                        "https://b3g1-amo-api.azurewebsites.net/callback"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:5011/",
                        "https://b3g1-amo-api.azurewebsites.net/"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles"
                    },
                    ClientSecrets =
                    {
                        new Secret("rookieamo".Sha256())
                    },
                    //AllowedCorsOrigins = new List<string>
                    //{
                    //    "https://localhost:5011/"
                    //},
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientName = "Rookie.Ecom.Web",
                    ClientId = "rookieecom",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RedirectUris = new List<string>()
                    {
                        "http://localhost:3000/callback",
                        "https://b3g1amo.z23.web.core.windows.net/callback"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "http://localhost:3000/",
                        "https://b3g1amo.z23.web.core.windows.net/"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles"
                    },
                    ClientSecrets =
                    {
                        new Secret("rookieecom".Sha256())
                    },
                    //AllowedCorsOrigins = new List<string>
                    //{
                    //    "http://localhost:3000",
                    //    "https://b3g1-amo-web.azurewebsites.net/callback"
                    //},
                    AllowAccessTokensViaBrowser = true
                },
                // m2m
                new Client
                {
                    ClientId = "client",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = { "roles" },
                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:5011",
                        "https://b3g1-amo-api.azurewebsites.net"
                    },
                },
                //angular app
                new Client {
                    ClientId = "angular",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    //RedirectUris = { "https://angular-ivy-qfiesp.stackblitz.io" },
                    //PostLogoutRedirectUris = { "https://angular-ivy-qfiesp.stackblitz.io" },
                    //AllowedCorsOrigins = { "https://angular-ivy-qfiesp.stackblitz.io" },

                    RedirectUris = { "http://localhost:4200" },
                    PostLogoutRedirectUris = { "http://localhost:4200" },
                    AllowedCorsOrigins = { "http://localhost:4200" },

                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles"
                    },

                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                },
            };
        }
    }
}
