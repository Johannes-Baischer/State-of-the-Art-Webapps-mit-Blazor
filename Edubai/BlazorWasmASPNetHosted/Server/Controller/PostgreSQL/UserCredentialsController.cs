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

namespace BlazorWasmHost.Controller.PostgreSQL
{
    /// <summary>
    /// API Controller for BlazorWasm that calls Methods as Server
    /// </summary>
    [ApiController]
    [Route("internal/[controller]/[Action]")]
    public class UserCredentialsController : ControllerBase
    {
        private SharedComponents.PostgreSQL.UserCredentialsController GetSharedController(JsonElement services)
        {
            IPlatformInfo platformInfo = services[0].Deserialize<WebPlatformInfo>();
            HttpClient httpClient = services[1].Deserialize<HttpClient>();
            
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 

            return new SharedComponents.PostgreSQL.UserCredentialsController(platformInfo, httpClient, null);
        }

        [HttpPost]
        [Authorize(Roles = "System, Student, Teacher")]
        [ActionName("Insert")]
        [ProducesResponseType(typeof(UserCredential), 201)]
        public async Task<IActionResult?> Internal_UserCredentials_Insert_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.PostgreSQL.UserCredentialsController ucc = GetSharedController(services);
            //calls Methode as Server (callerIsServer = true)

            UserCredential? uc = null;
            HttpStatusCode? statusCode = HttpStatusCode.Created;

            try
            {
                uc = await ucc.Insert(args[0].Deserialize<UserCredential>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, uc);
        }

        [HttpPost]
        [Authorize(Roles = "Student, Teacher")]
        [ActionName("Upsert")]
        [ProducesResponseType(typeof(UserCredential), 200)]
        public async Task<IActionResult?> Internal_UserCredentials_Upsert_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.PostgreSQL.UserCredentialsController ucc = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            UserCredential? uc = null;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                uc = await ucc.Upsert(args[0].Deserialize<UserCredential>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, uc);
        }

        [HttpPost]
        [Authorize(Roles = "System, Student, Teacher")]
        [ActionName("Read")]
        [ProducesResponseType(typeof(UserCredential), 200)]
        public async Task<IActionResult> Internal_UserCredentials_Read_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.PostgreSQL.UserCredentialsController ucc = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            UserCredential? uc = null;
            HttpStatusCode? statusCode = HttpStatusCode.OK;    

            try
            {
                uc = await ucc.Read(args[0].Deserialize<string>(), args[1].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex) 
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, uc);
        }

        [HttpPost]
        [Authorize(Roles = "System, Student, Teacher")]
        [ActionName("ReadSalt")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Internal_UserCredentials_ReadSalt_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.PostgreSQL.UserCredentialsController ucc = GetSharedController(services);


            //calls Methode as Server (callerIsServer = true)
            string? salt = null;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                salt = await ucc.ReadSalt(args[0].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, salt);
        }

        [HttpPost]
        [Authorize(Roles = "System, Student, Teacher")]
        [ActionName("CheckUserExists")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> Internal_UserCredentials_CheckUserExists_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.PostgreSQL.UserCredentialsController ucc = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            bool userExists = false;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                userExists = await ucc.CheckUserExists(args[0].Deserialize<string>(), true);
            }
            catch(HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, userExists);
        }

        [HttpPost]
        [Authorize(Roles = "System, Student, Teacher")]
        [ActionName("CheckPasswordResetTokenMatch")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> Internal_UserCredentials_CheckPasswordResetTokenMatch_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.PostgreSQL.UserCredentialsController ucc = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            bool tokenMatches = false;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                tokenMatches = await ucc.CheckPasswordResetTokenMatch(args[0].Deserialize<string>(), args[1].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, tokenMatches);
        }

        [HttpPost]
        [Authorize(Roles = "System, Student, Teacher")]
        [ActionName("ResetPassword")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> Internal_UserCredentials_ResetPassword_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.PostgreSQL.UserCredentialsController ucc = GetSharedController(services);

            //calls Methode as Server (callerIsServer = true)
            bool isOk = false;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                isOk = await ucc.ResetPassword(args[0].Deserialize<string>(), args[1].Deserialize<string>(), args[2].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, isOk);
        }
    }
}