using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SharedComponents.Configuration;
using SharedComponents.Pages.SignUp.Controller;
using SharedComponents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Pages.SignUp.Model
{
	public class SignUpBase : ComponentBase
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
        public string EmailVerificationToken { get; set; } = "";
        public string Password { get; set; } = "";

		public enum Roles
		{
			Student,
			Teacher
		}

		public Roles Role { get; set; } = Roles.Student;

		protected SignUpController Controller { get; set; } = default!;

		protected override async Task OnInitializedAsync()
		{
			Controller = new SignUpController(this);

			if(EmailVerificationToken != null)
			{
				EdubaiEmailController eec = new EdubaiEmailController(PlatformInfo, Http, AuthenticationStateProvider);
				bool success = await eec.VerifyEmail(Email, EmailVerificationToken);

				if(success)
				{
                    NavigationManager.NavigateTo("login");
                }
			}
		}
	}
}
