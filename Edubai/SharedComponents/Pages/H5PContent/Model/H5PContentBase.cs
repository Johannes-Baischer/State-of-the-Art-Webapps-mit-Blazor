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
using SharedComponents.Pages.LearningApps.Model;
using SharedComponents.Pages.LearningApps.Controller;

namespace SharedComponents.Pages.H5PContent.Model
{
    public class H5PContentBase : ComponentBase
    {
        #region Dependency Injection

        [Inject]
        public IPlatformInfo PlatformInfo { get; set; } = default!;

        [Inject]
        public HttpClient Http { get; set; } = default!;

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        public IJSObjectReference LearningAppsJSModule { get; set; } = default!;

        [Inject]
        public IStringLocalizer<SharedComponents.Localization.Localization> Localizer { get; set; } = default!;

        #endregion


        [Parameter]
        public string H5PID { get; set; } = default!;

        public LearningAppData? LearningAppData { get; set; } = null;

        protected override async Task OnInitializedAsync()
        {
            LearningAppsJSModule = await JS.InvokeAsync<IJSObjectReference>("import", "/_content/SharedComponents/Pages/H5PContent/View/H5PContent.razor.js");

            LearningAppsController Controller = new LearningAppsController(null!, PlatformInfo, Http, AuthenticationStateProvider, null!);

            // Get Metadata (IDs, Description) of all LearningApps from the directory they are stored in
            List<LearningAppData> LearningApps = await Controller.GetLearningApps("public", false) ?? new List<LearningAppData>();   // TODO: add logic for section of learning apps (public, specific school, ...)
            
            if (LearningApps == null)
            {
                EduBaiMessaging.ConsoleLog("LearningApps is null after call to server", "(1) Exception/H5PContent Base");
                EduBaiMessaging.Throw(new Exception("LearningApps is null after call to server"), "H5PContent Base", false);

                LearningAppData = new LearningAppData(H5PID, H5PID, "", null!, null!, null!);
            }
            else
            {
                LearningAppData = LearningApps.FirstOrDefault(x => x.ID == H5PID) ?? new LearningAppData(H5PID, H5PID, "", null!, null!, null!);
            }

            StateHasChanged();  // Rerender UI to include the h5p-container div, since at this point the LearningAppData is not null
            await LearningAppsJSModule.InvokeVoidAsync("addh5pcontent", LearningAppData.ID);

            await base.OnInitializedAsync();
        }
    }
}
