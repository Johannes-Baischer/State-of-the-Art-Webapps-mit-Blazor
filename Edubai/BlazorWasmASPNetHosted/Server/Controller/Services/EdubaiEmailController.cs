using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SharedComponents.Configuration;
using SharedComponents.PostgreSQL;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BlazorWasmHost.Controller.Services
{
    /// <summary>
    /// API Controller for BlazorWasm that calls Methods as Server
    /// </summary>
    [ApiController]
    [Route("internal/[controller]/[Action]")]
    public class EdubaiEmailController : ControllerBase
    {
        private SharedComponents.Services.EdubaiEmailController GetSharedController(JsonElement services) 
        {
            IPlatformInfo platformInfo = services[0].Deserialize<WebPlatformInfo>();
            HttpClient httpClient = services[1].Deserialize<HttpClient>();

            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return new SharedComponents.Services.EdubaiEmailController(platformInfo, httpClient, null);
        }

        [HttpPost]
        [ActionName("Send")]
        [Authorize(Roles = "System, Student, Teacher")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> Internal_Email_Send_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.Services.EdubaiEmailController ec = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            bool isOk = false;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                isOk = await ec.Send(args[0].Deserialize<string>(), args[1].Deserialize<string>(), args[2].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, isOk);
        }

        [HttpPost]
        [ActionName("SendPasswordReset")]
        [Authorize(Roles = "System, Student, Teacher")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> Internal_Email_SendPasswordReset_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.Services.EdubaiEmailController ec = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            bool isOk = false;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                isOk = await ec.SendPasswordReset(args[0].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, isOk);
        }

        [HttpPost]
        [ActionName("VerifyEmail")]
        [Authorize(Roles = "System, Student, Teacher")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> Internal_Email_VerifyEmail_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.Services.EdubaiEmailController ec = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            bool isOk = false;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                isOk = await ec.VerifyEmail(args[0].Deserialize<string>(), args[1].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, isOk);
        }
    }
}