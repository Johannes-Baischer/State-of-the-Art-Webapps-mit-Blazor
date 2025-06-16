using Blazored.LocalStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using SharedComponents.Development;
using SharedComponents.Services;
using System.Globalization;

namespace SharedComponents.Configuration
{
    /// <summary>
    /// Helper class to handle localization configuration.
    /// </summary>
    public class LocalizationInfo
	{
		private static LocalizationInfo _instance = default!;
		public static LocalizationInfo Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new LocalizationInfo(); 
				}

				return _instance;
			} 
		}

		/// <summary>
		/// List of supported languages.
		/// </summary>
		public enum Languages
		{
			en_US,
			de_AT,
		}

		/// <summary>
		///	List of all used keys in local storage
		/// </summary>
		public enum StorageTags
		{
			Culture
		}

		private Languages _language;
		public Languages Language
		{
			get { return _language; }
			set
			{
				_language = value;
			}
		}

		public string Code { get => LanguageToLanguageCode(Language); }


		/// <summary>
		/// Converts the enum Languages to the language code used by the CultureInfo class
		/// </summary>
		/// <param name="languages">Language in enum format</param>
		/// <returns>Languagecode as String (eg. de-AT)</returns>
		private string LanguageToLanguageCode(Languages languages)
		{
			return languages.ToString().Replace("_", "-");
		}

		private Languages LanguageCodeToLanguage(string languageCode)
		{
			return (Languages)Enum.Parse(typeof(Languages), languageCode.Replace("-", "_"));
		}

		/// <summary>
		/// Returns the RequestLocalizationOptions for the Startup with added supported languages
		/// </summary>
		/// <returns>RequestLocalizationOptions</returns>
		public RequestLocalizationOptions GetRequestLocalizationOptions()
		{
			//return ILocalizationInfo enum as string Array
			Languages[] languages = Enum.GetValues<Languages>();

			//replace underscore with dash
			string[] languagesWithDashes = new string[languages.Length];

			for (int i = 0; i < languages.Length; i++)
			{
				languagesWithDashes[i] = languages[i].ToString().Replace("_", "-");
			}

			var localizationOptions = new RequestLocalizationOptions()
				.AddSupportedCultures(languagesWithDashes)
				.AddSupportedUICultures(languagesWithDashes)
				.SetDefaultCulture(languagesWithDashes.FirstOrDefault("en-US"));

			return localizationOptions;
		}

		/// <summary>
		/// Sets the CultureInfo for the whole application
		/// </summary>
		/// <param name="PlatformInfo">PlatformInfo explicitly, since on startup there is no dependency injection available yet</param>
		/// <param name="culture">Culture info as enum value e.g. Languages.en_US</param>
		/// <returns>True if updating the Cultures localling and in local storage was successful</returns>
		public bool SetAllCultureInfos(Languages culture, IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!)
		{
			//Set LocalizationInfo Propertie, which in turn sets the language code
			Language = culture;

			CultureInfo.CurrentCulture =
			CultureInfo.CurrentUICulture =
			CultureInfo.DefaultThreadCurrentCulture =
			CultureInfo.DefaultThreadCurrentUICulture =
			Thread.CurrentThread.CurrentCulture =
			Thread.CurrentThread.CurrentUICulture =
			Localization.Localization.Culture = new CultureInfo(Code);

			//Set the language code in local storage/secure storage depending on the platform
			return EdubaiLocalStorage.Instance.Set(StorageTags.Culture.ToString(), Code, PlatformInfo, LocalStorage);
		}

		/// <summary>
		/// Sets the CultureInfo for the whole application from local storage
		/// </summary>
		/// <param name="PlatformInfo">PlatformInfo explicitly, since on startup there is no dependency injection available yet</param>
		/// <returns>The culture that was set e.g. en-US</returns>
		public bool SetAllCultureInfos(IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!)
		{
			//Get the language code from local storage/secure storage depending on the platform
			string cultureCode = EdubaiLocalStorage.Instance.Get(StorageTags.Culture.ToString(), LanguageToLanguageCode(Languages.en_US), PlatformInfo);

			return SetAllCultureInfos(LanguageCodeToLanguage(cultureCode), PlatformInfo, LocalStorage);
		}

		/// <summary>
		/// Sets the CultureInfo for the whole application
		/// </summary>
		/// <param name="PlatformInfo">PlatformInfo explicitly, since on startup there is no dependency injection available yet</param>
		/// <param name="culture">Culture info as enum value e.g. Languages.en_US</param>
		/// <returns>True if updating the Cultures localling and in local storage was successful</returns>
		public async Task<bool> SetAllCultureInfosAsync(Languages culture, IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!)
		{
			//Set LocalizationInfo Propertie, which in turn sets the language code
			Language = culture;

			CultureInfo.CurrentCulture =
			CultureInfo.CurrentUICulture =
			CultureInfo.DefaultThreadCurrentCulture =
			CultureInfo.DefaultThreadCurrentUICulture =
			Thread.CurrentThread.CurrentCulture =
			Thread.CurrentThread.CurrentUICulture =
			Localization.Localization.Culture = new CultureInfo(Code);

			//Set the language code in local storage/secure storage depending on the platform
			return await EdubaiLocalStorage.Instance.SetAsync(StorageTags.Culture.ToString(), Code, PlatformInfo, LocalStorage);
		}

		/// <summary>
		/// Sets the CultureInfo for the whole application from local storage
		/// </summary>
		/// <param name="PlatformInfo">PlatformInfo explicitly, since on startup there is no dependency injection available yet</param>
		/// <returns>The culture that was set e.g. en-US</returns>
		public async Task<bool> SetAllCultureInfosAsync(IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!)
		{
			string cultureCode;

			//Get the language code from local storage/secure storage depending on the platform
			cultureCode = await EdubaiLocalStorage.Instance.GetAsync(StorageTags.Culture.ToString(), LanguageToLanguageCode(Languages.en_US), PlatformInfo, LocalStorage);

			return await SetAllCultureInfosAsync(LanguageCodeToLanguage(cultureCode), PlatformInfo, LocalStorage);
		}

		/// <summary>
		/// Changes the language of the application (CALL THIS METHOD TO RUN THE WHOLE PROCEDURE INCLUDUNG SAVING TO LOCAL STORAGE)
		/// </summary>
		/// <param name="culture">Language supported/defined in Localizationinfo.Laguages</param>
		/// <param name="PlatformInfo"></param>
		/// <param name="LocalStorage">Required for Blazor WASM</param>
		/// <param name="NavigationManager">Required for Blazor WASM</param>
		/// <returns></returns>
		public async Task<bool> ChangeLanguage(Languages culture, IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!, NavigationManager NavigationManager = default!)
		{
			if (PlatformInfo.Platform == Platform.Maui)
			{
				//Call SetAllCultureInfos without LocalStorage
				return SetAllCultureInfos(culture, PlatformInfo);
			}
			else if(PlatformInfo.Platform == Platform.Web)
			{
				//Call SetAllCultureInfosAsync with LocalStorage
				bool success = await SetAllCultureInfosAsync(culture, PlatformInfo, LocalStorage);

				if (success)
				{
					//Reload to update Culture on Blazor Web
					NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
				}

				return success;
			}
			else
			{
				//Platform not specified
				EduBaiMessaging.ConsoleLog("Specified Platform not found!", "LocalizationInfo");
				return false;
			}
		}
	}

}
