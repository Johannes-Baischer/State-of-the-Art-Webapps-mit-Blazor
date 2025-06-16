using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.Configuration;
using SharedComponents.PostgreSQL;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorWasmHost.Controller.Services
{
    /// <summary>
    /// API Controller for BlazorWasm that calls Methods as Server
    /// </summary>
    [ApiController]
    [Route("internal/[controller]/[Action]")]
    public class EdubaiAESEncryptionController : ControllerBase
    {
        private SharedComponents.Services.EdubaiAESEncryptionController GetSharedController(JsonElement services)
        {
            IPlatformInfo platformInfo = services[0].Deserialize<WebPlatformInfo>();
            HttpClient httpClient = services[1].Deserialize<HttpClient>();

            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return new SharedComponents.Services.EdubaiAESEncryptionController(platformInfo, httpClient, null);
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName("Encrypt")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Internal_AESEncryption_Encrypt_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.Services.EdubaiAESEncryptionController aes = GetSharedController(services);

            string cipher = null;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                cipher = await aes.Encrypt(args[0].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }


            return StatusCode((int)statusCode, cipher);
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName("Decrypt")]
        [ProducesResponseType(typeof(string), 200)]

        public async Task<IActionResult> Internal_AESEncryption_Decrypt_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.Services.EdubaiAESEncryptionController aes = GetSharedController(services);


            string plain = null;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                plain = await aes.Decrypt(args[0].Deserialize<string>(), true);
            }
            catch (HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }
                
            return StatusCode((int)statusCode, plain);
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName("SignedJwt")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Internal_AESEncryption_SignedJwt_Post([FromBody] JsonElement body)
        {
            var services = body[0];
            var args = body[1];

            //calls SharedComponents Controller
            SharedComponents.Services.EdubaiAESEncryptionController aes = GetSharedController(services);

            List<Claim> claimsList = readClaimsFromJsonElement(args[0]);
            DateTime expires = args[1].Deserialize<DateTime>();
            string passwordHash =args[2].Deserialize<string>();

            string signedJwt = null;
            HttpStatusCode? statusCode = HttpStatusCode.OK;

            try
            {
                signedJwt = await aes.SignedJwt(claimsList, expires, passwordHash, true);
            }
            catch(HttpRequestException ex)
            {
                statusCode = ex.StatusCode;
            }

            return StatusCode((int)statusCode, signedJwt);
        }






        //---------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Custom JSON Parser for Claim class
        /// </summary>
        /// <param name="jsonElement"></param>
        /// <returns></returns>
        private List<Claim> readClaimsFromJsonElement(JsonElement jsonElement)
        {
            List<Claim> claims = new List<Claim>();

            string Issuer = null;
            string OriginalIssuer = null;
            object Properties = null;
            ClaimsIdentity Subject = null;
            string Type = null;
            string Value = null;
            string ValueType = null;

            for(int i = 0; i < jsonElement.GetArrayLength(); i++)
            {
                Issuer = jsonElement[i].GetProperty("Issuer").GetString();
                OriginalIssuer = jsonElement[i].GetProperty("OriginalIssuer").GetString();
                Properties = null;      //Add further parsing if needed
                Subject = null;         //Add further parsing if needed
                Type = jsonElement[i].GetProperty("Type").GetString();
                Value = jsonElement[i].GetProperty("Value").GetString();
                ValueType = jsonElement[i].GetProperty("ValueType").GetString();

                Claim c = new Claim(Type, Value, ValueType, Issuer, OriginalIssuer, Subject);
                claims.Add(c);
            }

            return claims;
        }
    }
}