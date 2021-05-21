using BlazorCat.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

//https://gist.github.com/Calabonga/6dc91b55cca59ef0ff68730d0d83cd2a
//https://docs.microsoft.com/ru-ru/aspnet/core/blazor/security/?view=aspnetcore-5.0
namespace BlazorCat
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddScoped<AuthenticationStateProvider, TokenAuthenticationStateProvider>();

            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }

    public class TokenAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;

        public TokenAuthenticationStateProvider(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }


        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            AuthenticationState CreateAnonymous()
            {
                var anonymousIdentity = new ClaimsIdentity();
                var anonymousPricipal = new ClaimsPrincipal(anonymousIdentity);
                return new AuthenticationState(anonymousPricipal);

            }

            var token = await _localStorageService.GetAsync<SecurityToken>(nameof(SecurityToken));

            if (token == null)
            {
               return CreateAnonymous();
            }
               
            if (string.IsNullOrEmpty(token.AccessToken) || token.ExpiredAt < DateTime.UtcNow)
            {
               return CreateAnonymous();
            }
            // Create real user state
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Country,"Russia"),
                new Claim(ClaimTypes.Name,token.UserName),
                new Claim(ClaimTypes.Expired,token.ExpiredAt.ToLongDateString()),
                new Claim(ClaimTypes.Role,"Administrator"),
                new Claim(ClaimTypes.Role,"Manager"),
                new Claim("Blazor","Rulez")
            };

            var identity = new ClaimsIdentity(claims, "Token");
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }
        public void MakeUserAnonymous()
        {
            _localStorageService.RemoveAsync(nameof(SecurityToken));
            var anonymousIdentity = new ClaimsIdentity();
            var anonymousPricipal = new ClaimsPrincipal(anonymousIdentity);
            var authState = Task.FromResult( new AuthenticationState(anonymousPricipal));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
