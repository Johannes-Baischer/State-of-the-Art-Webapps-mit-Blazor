using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using SharedComponents.Configuration;
using SharedComponents.Pages.ForgotPassword.Controller;
using SharedComponents.Pages.SignUp.Controller;
using SharedComponents.PostgreSQL;
using SharedComponents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SharedComponents.Pages.ForgotPassword.Model
{
    public class ForgotPasswordBase : ComponentBase
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

        [Inject]
        public IStringLocalizer<SharedComponents.Localization.Localization> Localizer { get; set; } = default!;

        #endregion

        [Parameter]
        [SupplyParameterFromQuery]
        public string Email { get; set; } = "";

        [Parameter]
        [SupplyParameterFromQuery]
        public string PasswordResetToken { get; set; } = "";

        public string NewPassword { get; set; } = default!;

        protected bool ShowPasswordResetForm { get; set; } = false;
        protected bool IsPasswordResetTokenValid { get; set; } = true;
        public bool PasswordResetEmailSent { get; set; } = false;

        protected ForgotPasswordController Controller { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Controller = new ForgotPasswordController(this);

            if (PasswordResetToken != null)
            {
                ShowPasswordResetForm = true;

                UserCredentialsController ucc = new UserCredentialsController(PlatformInfo, Http, AuthenticationStateProvider);
                IsPasswordResetTokenValid = await ucc.CheckPasswordResetTokenMatch(Email, PasswordResetToken);

                StateHasChanged();
            }
        }

        /// <summary>
        /// Public Proxy for StateHasChanged
        /// </summary>
        public new void StateHasChanged()
        {
            InvokeAsync(base.StateHasChanged);
        }

        /// <summary>
        /// Redirects to normal forgotpassword page.
        /// Used in case there is something wrong with the password reset token.
        /// </summary>
        public void RedirectToNonParametricPasswordReset() 
        {
            ShowPasswordResetForm = false;
            NavigationManager.NavigateTo("/forgotpassword");
        } 

        /// <summary>
        /// Redirects to the login page.
        /// Used in case of a successful password reset.
        /// </summary>
        public void RedirectToLogin()
        {
            NavigationManager.NavigateTo("/login");
        }
    }
}
