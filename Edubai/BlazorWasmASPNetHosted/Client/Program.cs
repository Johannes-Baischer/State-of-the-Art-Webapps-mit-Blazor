using Blazored.LocalStorage;
using BlazorWasmClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SharedComponents.Configuration;
using SharedComponents.Localization;
using System.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.Authorization;
using SharedComponents.Services;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using SharedComponents.Policies;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//Add Dependency Injection Services here
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IPlatformInfo, WebPlatformInfo>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLocalization();
builder.Services.AddScoped<AuthenticationStateProvider, EdubaiAuthStateProvider>();

//Authorization and Authentication
//builder.Services.AddAuthenticationCore();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("EmailIsVerified", policy =>
    {
        policy.AddRequirements(new EmailVerifiedRequirement("EmailIsVerified", new string[] { true.ToString() }));
    });
});

//Build the host seperately from running it to access the services for initialization (e.g. LocalStorage)
var host = builder.Build();

await LocalizationInfo.Instance.SetAllCultureInfosAsync(new WebPlatformInfo(), host.Services.GetRequiredService<ILocalStorageService>());

await host.RunAsync();
