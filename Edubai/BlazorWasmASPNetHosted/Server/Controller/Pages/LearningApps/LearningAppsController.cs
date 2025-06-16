using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SharedComponents.Configuration;
using SharedComponents.PostgreSQL;
using SharedComponents.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using SharedComponents.Pages.LearningApps.Model;

namespace BlazorWasmHost.Controller.Pages.LearningApps
{
    /// <summary>
    /// API Controller for BlazorWasm that calls Methods as Server
    /// </summary>
    [ApiController]
    [Route("internal/[controller]/[Action]")]
    public class LearningAppsController : ControllerBase
    {
        private SharedComponents.Pages.LearningApps.Controller.LearningAppsController GetSharedController(JsonElement services)
        {
            IPlatformInfo platformInfo = services[0].Deserialize<WebPlatformInfo>();
            HttpClient httpClient = services[1].Deserialize<HttpClient>();
            
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 

            return new SharedComponents.Pages.LearningApps.Controller.LearningAppsController(null, platformInfo, httpClient, null, null);
        }

        [HttpPost]
        [Authorize(Roles = "System, Student, Teacher")]
        [ActionName("GetLearningApps")]
        [ProducesResponseType(typeof(List<LearningAppData>), 200)]
        public async Task<IActionResult> Internal_LearningApps_GetLearningApps_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.Pages.LearningApps.Controller.LearningAppsController lac = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            List<LearningAppData>? lapps = null;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                lapps = await lac.GetLearningApps(args[0].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex) 
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, lapps);
        }

        [HttpPost]
        [Authorize(Roles = "System, Student, Teacher")]
        [ActionName("GetFeaturedLearningApps")]
        [ProducesResponseType(typeof(List<LearningAppData>), 200)]
        public async Task<IActionResult> Internal_LearningApps_GetFeaturedLearningApps_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.Pages.LearningApps.Controller.LearningAppsController lac = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            List<string>? featlapps = default!;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                featlapps = await lac.GetFeaturedLearningApps(args[0].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, featlapps);
        }
    }
}