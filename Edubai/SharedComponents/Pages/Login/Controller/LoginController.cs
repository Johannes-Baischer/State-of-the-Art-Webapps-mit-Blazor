using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using SharedComponents.Configuration;
using SharedComponents.Development;
using SharedComponents.Pages.Login.Model;
using SharedComponents.PostgreSQL;
using SharedComponents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Pages.Login.Controller
{
    public class LoginController
	{
		private LoginBase Model { get; set; }

		public LoginController(LoginBase model)
		{
			Model = model;
		}

		/// <summary>
		/// Logs in a user with the given credentials
		/// </summary>
		public async Task Login(bool redirect = true)
		{
			UserCredentialsController ucc = new UserCredentialsController(Model.PlatformInfo, Model.Http, Model.AuthenticationStateProvider);

			//TODO: Loading Spinner

			//------------------Get the user credential from the database------------------
			string? passwordSalt = await ucc.ReadSalt(Model.Email);
			if (passwordSalt == null)
			{
				//TODO: Show error Toast
				EduBaiMessaging.ConsoleLog("Login failed! User not found!", "Login");
			}
			else
			{
				string hashedGivenPassword = UserCredentialsController.HashPasswordWithSalt(Model.Password, passwordSalt);

				UserCredential? userCredential = await ucc.Read(Model.Email, hashedGivenPassword);

				if (userCredential == null)
				{
					EduBaiMessaging.ConsoleLog("Login failed! Password incorrect!", "Login");
				}
				else
				{
					List<Claim> claims = new List<Claim>
					{
                        new Claim(ClaimTypes.Name, Model.Email),
                        new Claim(ClaimTypes.Role, userCredential.Role.ToString()),
						new Claim("EmailIsVerified", userCredential.EmailIsVerified.ToString())
                    };

					EdubaiAuthStateProvider easp = (EdubaiAuthStateProvider)Model.AuthenticationStateProvider;    //cast to EdubaiAuthStateProvider to access SetJwtFromClaims
					await easp.GetAuthenticationStateAsync(claims, hashedGivenPassword);
                    await easp.SetJwtFromClaims(claims, hashedGivenPassword);


                    EduBaiMessaging.ConsoleLog("Logged in as " + Model.Role.ToString() + " " + Model.Email, "Login");

					if (redirect)
						Model.NavigationManager.NavigateTo("/learningapps");
				}
			}
		}
	}
}
