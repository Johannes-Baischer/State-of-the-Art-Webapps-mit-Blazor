using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace SharedComponents.Pages.Startup.Model
{
    public class StartupBase : ComponentBase
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IAuthorizationService AuthorizationService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            AuthenticationState authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            if (authenticationState != null && !authenticationState.User.IsInRole("System"))
            {
                // User is authenticated and is not a system user

                if((await AuthorizationService.AuthorizeAsync(authenticationState.User, "EmailIsVerified")).Succeeded == false)
                {
                    // User did not verfy its email yet
                    NavigationManager.NavigateTo("/verifyyouremail");
                }
                else
                {
                    NavigationManager.NavigateTo("/learningapps");
                }
            }
            else
            {
                NavigationManager.NavigateTo("/login");
            }
        }
    }
}
