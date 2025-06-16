using SharedComponents.Pages.ForgotPassword.Model;
using SharedComponents.Pages.SignUp.Model;
using SharedComponents.PostgreSQL;
using SharedComponents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Pages.ForgotPassword.Controller
{
    public class ForgotPasswordController
    {
        private ForgotPasswordBase Model { get; set; }

        public ForgotPasswordController(ForgotPasswordBase model)
        {
            Model = model;
        }

        /// <summary>
        /// Generates a password reset token and sends an email to the user with a link to reset their password.
        /// </summary>
        public async Task SendPasswordResetEmail()
        {
            UserCredentialsController ucc = new UserCredentialsController(Model.PlatformInfo, Model.Http, Model.AuthenticationStateProvider);

            bool userExists = await ucc.CheckUserExists(Model.Email);

            if (!userExists)
            {
                // Todo Error Toast User does not exists
                return;
            }

            EdubaiEmailController eec = new EdubaiEmailController(Model.PlatformInfo, Model.Http, Model.AuthenticationStateProvider);

            // Redirect to Login
            await eec.SendPasswordReset(Model.Email);

            Model.PasswordResetEmailSent = true;
            Model.StateHasChanged();
        }

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        public async Task ResetPassword()
        {
            UserCredentialsController ucc = new UserCredentialsController(Model.PlatformInfo, Model.Http, Model.AuthenticationStateProvider);

            string PasswordSalt = await ucc.ReadSalt(Model.Email);
            if(PasswordSalt == null)
            {
                // todo: error toast couldnt read from userdata
                return;
            }

            string NewPasswordHash = UserCredentialsController.HashPasswordWithSalt(Model.NewPassword, PasswordSalt);

            bool success = await ucc.ResetPassword(Model.Email, NewPasswordHash, Model.PasswordResetToken);

            if (success)
            {
                Model.RedirectToLogin();
            }
        }
    }
}
