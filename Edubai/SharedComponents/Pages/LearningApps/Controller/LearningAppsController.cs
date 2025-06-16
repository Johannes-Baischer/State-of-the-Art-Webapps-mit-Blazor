using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using SharedComponents.Configuration;
using SharedComponents.Development;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SharedComponents.Pages.LearningApps.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace SharedComponents.Pages.LearningApps.Controller
{
    public class LearningAppsController
	{
        private const string ROUTEASAPICONTROLLER = "internal/LearningApps";
        private IPlatformInfo? PlatformInfo { get; set; } = default!;
        private HttpClient? Http { get; set; } = default!;
        private AuthenticationStateProvider? AuthenticationStateProvider { get; set; } = default!;
        private NavigationManager? NavigationManager { get; set; } = default!;
        private LearningAppsBase Model { get; set; }

        /// <summary>
        /// Constructor with only the model as parameter for the case that the controller is created in a razor page
        /// </summary>
        /// <param name="model">Model</param>
        public LearningAppsController(LearningAppsBase model)
        {
            Model = model;
            Http = Model.Http;
            PlatformInfo = Model.PlatformInfo;
            AuthenticationStateProvider = Model.AuthenticationStateProvider;
            NavigationManager = Model.NavigationManager;
            Model.AuthenticationStateProvider = AuthenticationStateProvider;
        }

        /// <summary>
        /// Contructor Injection because [inject] only in cs files connected to a razor page
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="platformInfo">Current Platform
        /// Nullable for calls with no access to PlatformInfo (e.g. in BlazorWasmHost)</param>
        /// <param name="http">HttpClient, most likely from dependency injection
        /// Nullable for calls with no access to PlatformInfo (e.g. in BlazorWasmHost)</param>
        /// <param name="authenticationStateProvider">AuthenticationStateProvider, used to get permissions of current user</param>
        public LearningAppsController(LearningAppsBase model, IPlatformInfo? platformInfo, HttpClient? http, AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager)
        {
            Model = model;
            PlatformInfo = platformInfo;
            Http = http;
            AuthenticationStateProvider = authenticationStateProvider;
            NavigationManager = navigationManager;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------
        // API Calls (Server)

        /// <summary>
        /// Get the IDs (=Names) of all LearningApps from the directory they are stored in
        /// </summary>
        /// <param name="section">The section from where to read the learningapps (e.g. "public" or a specific school)</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns>List of LearningAppData</returns>
        public async Task<List<LearningAppData>?> GetLearningApps(string section, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                string[] args = { section };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/GetLearningApps", content);

                List<LearningAppData>? lapps = default;
                //Successfully called by the server
                try
                {
                    lapps = await response.Content.ReadFromJsonAsync<List<LearningAppData>?>();
                }
                catch
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(1) HTTP Error! Internal Server Error", "LearningApps Controller");
                    return null;
                }

                return lapps;
            }
            #endregion

            List<LearningAppData>? apps = null;

            try
            {
                Dictionary<string, string> HostingPaths = new Dictionary<string, string>()
                {
                    { "/var/www/edubai", "./wwwroot/_content/SharedComponents/H5PContent/" + section },
                    { "C:/GitHub/Personal/Edubai/BlazorWasmASPNetHosted/Server", "../../SharedComponents/wwwroot/H5PContent/" + section},
                    { "C:/GitHub/Personal/Edubai/BlazorMaui", "../SharedComponents/wwwroot/H5PContent/" + section}
                    //TODO: find path for published BlazorMaui app
                };

                if (Model?.PlatformInfo.Platform == Platform.Maui)
                {
                    // special case for BlazorMaui, where the current directory is different for every platform
                    string[] basePathParts = AppDomain.CurrentDomain.BaseDirectory.Split("\\bin\\");
                    Directory.SetCurrentDirectory(basePathParts[0] ?? "C:/");
                }

                var relativePaths = HostingPaths.Values;

                foreach(var rp in relativePaths)
                {
                    //to allow the different Hostingpaths to be moved around relative to their environment,
                    //we only try the relative paths and look for any "DirectoryNotFoundException"
                    //the first Directory that is found will be used

                    try
                    {
                        //Get all names of folders in the directory (= LearningApps)
                        string path = Path.Combine(Directory.GetCurrentDirectory(), rp);
                        DirectoryInfo[] dirs = new DirectoryInfo(path).GetDirectories();

                        apps = new List<LearningAppData>();
                        foreach (DirectoryInfo dir in dirs)
                        {
                            JsonElement? h5pjson = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(dir.FullName + "/h5p.json"));
                            string authorComments = h5pjson?.GetProperty("authorComments").GetString() ?? string.Empty;

                            // if there is no valid authorComments field, throw an Exception
                            if (authorComments == string.Empty)
                            {
                                throw new FormatException("No AuthorComments were found in the h5p.json File. This might be due to an Error in decoding.");
                            }
                            else
                            {
                                apps.Add(new LearningAppData(authorComments));
                            }
                        }

                        break;  //if no Exception accured up to this point, the directory is valid
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        // skip Exception loging, since this is somewhat expected 
                        //EduBaiMessaging.ConsoleLog(ex.ToString(), "(2) Exception/LearningApps Controller");
                        //EduBaiMessaging.Throw(ex, "LearningApps Controller", false);
                    }
                    catch (Exception ex)
                    {
                        EduBaiMessaging.ConsoleLog(ex.ToString(), "(3) Exception/LearningApps Controller");
                        EduBaiMessaging.Throw(ex, "LearningApps Controller", callerIsServer);
                    }
                }
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog(ex.ToString(), "(4) Exception/LearningApps Controller");
                EduBaiMessaging.Throw(ex, "LearningApps Controller", callerIsServer);
                return null;
            }

            if (apps == null)
            {
                //Email not found
                EduBaiMessaging.ConsoleLog($"(5) Error while reading LearningApps", "LearningApps Controller");
                EduBaiMessaging.Throw(new HttpRequestException($"(5) Error while reading LearningApps", null, HttpStatusCode.NotFound), "LearningApps Controller", callerIsServer);
                return null;
            }

            return apps;
        }

        /// <summary>
        /// Get the IDs (=Names) of all LearningApps from the directory they are stored in
        /// </summary>
        /// <param name="section">The section from where to read the learningapps (e.g. "public" or a specific school)</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns>List of LearningAppData</returns>
        public async Task<List<string>?> GetFeaturedLearningApps(string section, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                string[] args = { section };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/GetFeaturedLearningApps", content);

                List<string>? featlapps = default;
                //Successfully called by the server
                try
                {
                    featlapps = await response.Content.ReadFromJsonAsync<List<string>?>();
                }
                catch
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(1) HTTP Error! Internal Server Error", "LearningApps Controller");
                    return null;
                }

                return featlapps;
            }
            #endregion

            List<string>? featApps = null;

            try
            {
                Dictionary<string, string> HostingPaths = new Dictionary<string, string>()
                {
                    { "/var/www/edubai", "./wwwroot/_content/SharedComponents/H5PContent/" + section },
                    { "C:/GitHub/Personal/Edubai/BlazorWasmASPNetHosted/Server", "../../SharedComponents/wwwroot/H5PContent/" + section }
                };

                var relativePaths = HostingPaths.Values;

                foreach (var rp in relativePaths)
                {
                    //to allow the different Hostingpaths to be moved around relative to their environment,
                    //we only try the relative paths and look for any "DirectoryNotFoundException"
                    //the first Directory that is found will be used

                    try
                    {
                        //Get all names of folders in the directory (= LearningApps)
                        string path = Path.Combine(Directory.GetCurrentDirectory(), rp);
                        DirectoryInfo[] dirs = new DirectoryInfo(path).GetDirectories();

                        featApps = new List<string>();
                        
                        JsonElement? h5pjson = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(path + "/featuredlearningapps.json"));
                        
                        foreach (var id in h5pjson?.GetProperty("ids").EnumerateArray())
                        {
                            featApps.Add(id.GetString());
                        }

                        // if there is no valid authorComments field, throw an Exception
                        if (featApps.Count == 0)
                        {
                            EduBaiMessaging.ConsoleLog("No featured apps found in the featuredlearningapps.json File. This might be due to an Error in decoding.");
                        }

                        break;  //if not Exception accured up to this point, the directory is valid
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        // skip Exception loging, since this is somewhat expected 
                        //EduBaiMessaging.ConsoleLog(ex.ToString(), "(2) Exception/LearningApps Controller");
                        //EduBaiMessaging.Throw(ex, "LearningApps Controller", false);
                    }
                    catch (Exception ex)
                    {
                        EduBaiMessaging.ConsoleLog(ex.ToString(), "(3) Exception/LearningApps Controller");
                        EduBaiMessaging.Throw(ex, "LearningApps Controller", callerIsServer);
                    }
                }
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog(ex.ToString(), "(4) Exception/LearningApps Controller");
                EduBaiMessaging.Throw(ex, "LearningApps Controller", callerIsServer);
                return null;
            }

            if (featApps == null)
            {
                //Email not found
                EduBaiMessaging.ConsoleLog($"(5) Error while reading featured LearningApps", "LearningApps Controller");
                EduBaiMessaging.Throw(new HttpRequestException($"(5) Error while reading featured LearningApps", null, HttpStatusCode.NotFound), "LearningApps Controller", callerIsServer);
                return null;
            }

            return featApps;
        }
    }
}
