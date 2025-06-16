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
using SharedComponents.Pages.LearningApps.Controller;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;
using static System.Collections.Specialized.BitVector32;

namespace SharedComponents.Pages.LearningApps.Model
{
    public class LearningAppsBase : ComponentBase
    {
        #region Dependency Injection

        [Inject]
        public IPlatformInfo PlatformInfo { get; set; } = default!;

        [Inject]
        public HttpClient Http { get; set; } = default!;

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public IStringLocalizer<SharedComponents.Localization.Localization> Localizer { get; set; } = default!;

        #endregion

        protected List<LearningAppData>? LearningApps { get; set; } = default!;
        protected List<LearningAppData>? FeaturedLearningApps { get; set; } = default!;
        protected LearningAppData SelectedLearningApp { get; set; } = default!;
        protected LearningAppsController Controller { get; set; } = default!;
        protected override async Task OnInitializedAsync()
        {
            Controller = new LearningAppsController(this, PlatformInfo, Http, AuthenticationStateProvider, NavigationManager);

            // Get Metadata (IDs, Description) of all LearningApps from the directory they are stored in
            LearningApps = await Controller.GetLearningApps("public", false);   // TODO: add logic for section of learning apps (public, specific school, ...)
            if (LearningApps == null)
            {
                EduBaiMessaging.ConsoleLog("LearningApps is null after call to server", "(1) Exception/LearningApps Base");
                EduBaiMessaging.Throw(new Exception("LearningApps is null after call to server"), "LearningApps Base", false);

                LearningApps = new List<LearningAppData>();
                FeaturedLearningApps = new List<LearningAppData>();
            }
            else
            {
                // Get featured learningapps
                List<string>? featured = await Controller.GetFeaturedLearningApps("public", false) ?? new List<string>();
                FeaturedLearningApps = new List<LearningAppData>();

                foreach(LearningAppData lapp in LearningApps)
                {
                    if (featured.Contains(lapp.ID))
                    {
                        FeaturedLearningApps.Add(lapp);
                    }
                }

                //select one random app to be featured in the "have you tried" - section
                if(LearningApps.Count > 0)
                {
                    int selectedAppIndex = new Random().Next(0, LearningApps.Count);
                    SelectedLearningApp = LearningApps[selectedAppIndex];
                }
            }

            await base.OnInitializedAsync();
        }
    }
}
