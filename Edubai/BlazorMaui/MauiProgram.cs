using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedComponents.Configuration;
using SharedComponents.Policies;
using SharedComponents.Services;
using System.Reflection;
using System.Security.Claims;

namespace BlazorMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
            //Add your services here
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7226/") });
            builder.Services.AddSingleton<IPlatformInfo, MauiPlatformInfo>();
			builder.Services.AddBlazoredLocalStorage(); //Not used as a service, but added for compatibility with Shared Components with Blazor Web
            builder.Services.AddLocalization();
            builder.Services.AddScoped<AuthenticationStateProvider, EdubaiAuthStateProvider>();


            //Authorization and Authentication
            builder.Services.AddAuthenticationCore();
            builder.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("EmailIsVerified", policy =>
                {
                    policy.AddRequirements(new EmailVerifiedRequirement("EmailIsVerified", new string[] { true.ToString() }));
                });
            });


            //Set Culture Info of the entire application from secure storage
            LocalizationInfo.Instance.SetAllCultureInfos(new MauiPlatformInfo());

			return builder.Build();
        }
    }
}