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

namespace SharedComponents.Pages.Dashboard.Model
{
    public class LearningAppsBase : ComponentBase
    {
        #region Dependency Injection

        [Inject]
        public IPlatformInfo PlatformInfo { get; set; } = default!;

        [Inject]
        public HttpClient Http { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        #endregion
    }
}
