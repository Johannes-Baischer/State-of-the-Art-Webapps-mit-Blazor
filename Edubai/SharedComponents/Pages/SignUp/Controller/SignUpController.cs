using SharedComponents.Services;
using SharedComponents.Development;
using SharedComponents.Pages.Login.Model;
using SharedComponents.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedComponents.Pages.SignUp.Model;
using SharedComponents.Pages.Login.Controller;

namespace SharedComponents.Pages.SignUp.Controller
{
	public class SignUpController
	{
		private SignUpBase Model { get; set; }

		public SignUpController(SignUpBase model)
		{
			Model = model;
		}


		/// <summary>
		/// Creates a new user in the database
		/// </summary>
		public async void CreateNewUser()
		{
			//------------------Create a new user credential to send in the POST request------------------

			// Salt
			string salt = UserCredentialsController.GenerateSalt();

			// Password
			string passwordHash = UserCredentialsController.HashPasswordWithSalt(Model.Password, salt);

			// UserCredentials
			var newUserCredential = new UserCredential
			{
				Email = Model.Email,
				PasswordHash = passwordHash,
				PasswordSalt = salt,
				EmailIsVerified = false,
				EmailVerificationToken = UserCredentialsController.GenerateSalt(),
				Role = Model.Role.ToString(),
			};

			UserCredential? uc = await new UserCredentialsController(Model.PlatformInfo, Model.Http, Model.AuthenticationStateProvider).Insert(newUserCredential);
			
			if (uc == null)
			{
                // Error or User already exists
                return;
            }
			await Login(Model.Email, Model.Password);

			string verificationLink = "<a href=\"" + Model.Http.BaseAddress + "signup?Email=" + Model.Email + "&EmailVerificationToken=" + newUserCredential.EmailVerificationToken + "\"> Click here to verify your email </a>";

            // Redirect to Login
            await new EdubaiEmailController(Model.PlatformInfo, Model.Http, Model.AuthenticationStateProvider).Send(
				Model.Email,
				"Edubai - Account Created",
                "Your account has been created successfully! <br />" +
                "Please verify your Email by clicking on the following link: <br />" +
				verificationLink
			);
			Model.NavigationManager.NavigateTo("/login");
		}


		/// <summary>
		/// Tries to log in the user after creating the account, using the LoginController component
		/// </summary>
		/// <param name="Email"></param>
		/// <param name="Password"></param>
		/// <returns></returns>
		private async Task Login(string Email, string Password)
		{
			LoginBase loginBase = new LoginBase();
			loginBase.Email = Email;
			loginBase.Password = Password;
			loginBase.PlatformInfo = Model.PlatformInfo;
			loginBase.Http = Model.Http;
			loginBase.AuthenticationStateProvider = Model.AuthenticationStateProvider;

			LoginController loginController = new LoginController(loginBase);

			await loginController.Login(false);
		}
	}
}
