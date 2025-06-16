using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Storage;
using SharedComponents.Configuration;
using SharedComponents.Development;

namespace SharedComponents.Services
{
    /// <summary>
    /// Used to store information in local storage.
    /// Takes into account the platform currently used (Maui, WebAssembly...).
    /// </summary>
    public class EdubaiLocalStorage : ComponentBase
    {
        private static EdubaiLocalStorage _instance = default!;

        //private static dynamic Preferences;   //Line used for dotnet ef update without maui references

        /// <summary>
        /// Static instance of the class.
        /// </summary>
        public static EdubaiLocalStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EdubaiLocalStorage();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Sets the key value pair in the localstorage depending on the platform
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="PlatformInfo">Maui, Web, ...</param>
        /// <param name="LocalStorage">BlazoredStorage, used for Blazor Web</param>
        /// <returns></returns>
        public bool Set(string key, string value, IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!)
        {
            try
            {
                //Set the language in local storage
                if (PlatformInfo.Platform == Platform.Web)
                {
                    EduBaiMessaging.Throw(new PlatformNotSupportedException("Calling synchronous LocalStorage is not Supported on this Platform! Try using SetAsync"), "EdubaiLocalStorage");
                    return false;
                }
                else if (PlatformInfo.Platform == Platform.Maui)
                {
                    //Save Culture info to secure storage
                    Preferences.Default.Set(key, value);

                    return true;
                }
                else
                {
                    EduBaiMessaging.ConsoleLog("Specified Platform not found!", "LocalStorage");
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Writing to local storage failed
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/LocalStorage");
                return false;
            }
        }

        /// <summary>
        /// Sets the key value pair in the localstorage depending on the platform
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="PlatformInfo">Maui, Web, ...</param>
        /// <param name="LocalStorage">BlazoredStorage, used for Blazor Web</param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, string value, IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!)
        {
            try
            {
                //Set the language in local storage
                if (PlatformInfo.Platform == Platform.Web)
                {
                    if (LocalStorage != null)
                    {
                        await LocalStorage.SetItemAsync(key, value);

                        return true;
                    }
                    else
                    {
                        EduBaiMessaging.ConsoleLog("No LocalStorage specified", "LocalStorage");
                        return false;
                    }

                }
                else if (PlatformInfo.Platform == Platform.Maui)
                {
                    EduBaiMessaging.ConsoleLog("Setting the Localstorage asynchronously on " + PlatformInfo.Platform.ToString() + "is discouraged. Use \"Set\" instead.", "LocalStorage");

                    //Save Culture info to secure storage
                    Preferences.Default.Set(key, value);

                    return true;
                }
                else
                {
                    EduBaiMessaging.ConsoleLog("Specified Platform not found!", "LocalStorage");
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Writing to local storage failed
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/LocalStorage");
                return false;
            }
        }

        /// <summary>
        /// Gets the value from the localstorage depending on the platform
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue">Is returned in case there is no value found for the given key</param>
        /// <param name="PlatformInfo">Information about the current platform (Maui, Web...)</param>
        /// <param name="LocalStorage">Reference to LocalStorage Service in case of Blazor Web</param>
        /// <returns>The value from the localstorage with the given key</returns>
        public string Get(string key, string defaultValue, IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!)
        {
            try
            {
                //Get the language from local storage
                if (PlatformInfo.Platform == Platform.Web)
                {
                    EduBaiMessaging.Throw(new PlatformNotSupportedException("Calling synchronous LocalStorage is not Supported on this Platform! Try using SetAsync"), "EdubaiLocalStorage");
                    return defaultValue;
                }
                else if (PlatformInfo.Platform == Platform.Maui)
                {
                    //get Culture info from secure storage
                    return Preferences.Default.Get(key, defaultValue);
                }
                else
                {
                    EduBaiMessaging.ConsoleLog("Specified Platform not found!", "LocalStorage");
                    return defaultValue;
                }
            }
            catch (Exception ex)
            {
                //Writing to local storage failed
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/LocalStorage");
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the value from the localstorage depending on the platform
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue">Is returned in case there is no value found for the given key</param>
        /// <param name="PlatformInfo">Information about the current platform (Maui, Web...)</param>
        /// <param name="LocalStorage">Reference to LocalStorage Service in case of Blazor Web</param>
        /// <returns>The value from the localstorage with the given key</returns>
        public async Task<string> GetAsync(string key, string defaultValue, IPlatformInfo PlatformInfo, ILocalStorageService LocalStorage = default!)
        {
            try
            {
                //Set the language in local storage
                if (PlatformInfo.Platform == Platform.Web)
                {
                    if (LocalStorage != null)
                    {
                        string result = await LocalStorage.GetItemAsync<string>(key);

                        return result ?? defaultValue;
                    }
                    else
                    {
                        EduBaiMessaging.ConsoleLog("No LocalStorage specified", "LocalStorage");
                        return defaultValue;
                    }
                }
                else if (PlatformInfo.Platform == Platform.Maui)
                {
                    EduBaiMessaging.ConsoleLog("Getting form the Localstorage asynchronously on " + PlatformInfo.Platform.ToString() + "is discouraged. Use \"Get\" instead.", "LocalStorage");
                    //Save Culture info to secure storage
                    return Preferences.Default.Get(key, defaultValue);
                }
                else
                {
                    EduBaiMessaging.ConsoleLog("Specified Platform not found!", "LocalStorage");
                    return defaultValue;
                }
            }
            catch (Exception ex)
            {
                //Writing to local storage failed
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/Localstorage");
                return defaultValue;
            }
        }
    }
}
