using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using SharedComponents.Configuration;

namespace SharedComponents.Pages.VerifyYourEmail.Model
{
    public class VerifyYourEmailBase : ComponentBase
    {
        #region Dependency Injection

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IAuthorizationService AuthorizationService { get; set; } = default!;

        [Inject]
        public IStringLocalizer<SharedComponents.Localization.Localization> Localizer { get; set; } = default!;

        #endregion

        protected string Email { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AuthenticationState authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            if (authenticationState != null && !authenticationState.User.IsInRole("System"))
            {
                // User is authenticated and is not a system user
                if ((await AuthorizationService.AuthorizeAsync(authenticationState.User, "EmailIsVerified")).Succeeded)
                {
                    NavigationManager.NavigateTo("/learningapps");
                }

                Email = authenticationState.User.Identity?.Name;
            }
            else
            {
                NavigationManager.NavigateTo("/login");
            }
        }
    }
}
