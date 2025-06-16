using Microsoft.AspNetCore.Components;
using SharedComponents.Configuration;
using SharedComponents.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharedComponents.Development;
using Microsoft.JSInterop;
using SharedComponents.Pages.Login.Controller;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace SharedComponents.Pages.Login.Model
{
    public class LoginBase : ComponentBase
    {
        #region Dependency Injection
        [Inject]
        public IPlatformInfo PlatformInfo { get; set; } = default!;

        [Inject]
		public HttpClient Http { get; set; } = default!;

        [Inject]
		public IJSRuntime JS { get; set; } = default!;

		[Inject]
		public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ILocalStorageService LocalStorage { get; set; } = default!;

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        public IStringLocalizer<SharedComponents.Localization.Localization> Localizer { get; set; } = default!;

        public IJSObjectReference LoginJSModule { get; set; } = default!;

		#endregion

		public string Email { get; set; } = "";
		public string Password { get; set; } = "";

        public enum Roles
        {
            Student,
            Teacher
        }

        public Roles Role { get; set; } = Roles.Student;

        protected LoginController Controller { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Controller = new LoginController(this);

            //registers mouse listeners etc. for css animations
            LoginJSModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/SharedComponents/Pages/Login/View/Login.razor.js");
            await LoginJSModule.InvokeVoidAsync("registerLoginCardEffects");

            await base.OnInitializedAsync();
        }
    }
}
