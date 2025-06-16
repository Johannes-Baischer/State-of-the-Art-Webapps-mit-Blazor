using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using SharedComponents.Configuration;
using Microsoft.Extensions.Configuration;
using SharedComponents.Development;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using SharedComponents.PostgreSQL;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Text.Json.Serialization;

namespace SharedComponents.Services
{
	public class EdubaiAuthStateProvider : AuthenticationStateProvider
	{
        private IPlatformInfo PlatformInfo { get; set; } = default!;
        private ILocalStorageService LocalStorage { get; set; } = default!;
        private HttpClient Http { get; set; } = default!;

        /// <summary>
        /// Constructor that gets called by the Engine, resolving all parameters to registered Services of the same Type.
        /// </summary>
        /// <param name="localStorage"></param>
        /// <param name="http"></param>
        /// <param name="platformInfo"></param>
		public EdubaiAuthStateProvider(ILocalStorageService localStorage, HttpClient http, IPlatformInfo platformInfo)
		{
            this.LocalStorage = localStorage;
            this.Http = http;
			this.PlatformInfo = platformInfo;
        }

		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
            return await GetAuthenticationStateAsync(null, null);
        }

        public async Task<AuthenticationState> GetAuthenticationStateAsync(IEnumerable<Claim>? claims, string passwordHash)
        {
            string token = null;

            if(claims == null)
            {
                if (PlatformInfo.Platform == Platform.Maui)
                {
                    token = EdubaiLocalStorage.Instance.Get("edubaiauthstatetoken", "", PlatformInfo);
                }
                else
                {
                    token = await EdubaiLocalStorage.Instance.GetAsync("edubaiauthstatetoken", "", PlatformInfo, LocalStorage);
                }

                token = await GetValidOrDefault(token);

                // Decrypt the JWT
                EdubaiAESEncryptionController aes = new EdubaiAESEncryptionController(PlatformInfo, Http, this);
                token = await aes.Decrypt(token);

                claims = await ParseClaimsFromJwt(token);
            }

            if (token == null)
            {
                token = await GenerateJwtFromClaims(claims, passwordHash);
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, "jwt");
            Http.DefaultRequestHeaders.Authorization = null;
            Http.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token!.Replace("\"", ""));


            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }

        /// <summary>
        /// Sets the Jwt in local storage from the given claims
        /// </summary>
        /// <param name="claims">Enumerable Collection of Claims</param>
        /// <returns>true, if the operation was successfull</returns>
        public async Task<bool> SetJwtFromClaims(IEnumerable<Claim> claims, string passwordHash)
        {
			bool success = false;

			string token = await GenerateJwtFromClaims(claims, passwordHash);
            // Encrypt the JWT
            EdubaiAESEncryptionController aes = new EdubaiAESEncryptionController(PlatformInfo, Http, this);
            string encryptedToken = await aes.Encrypt(token);


			// Save token to local storage
            if (PlatformInfo.Platform == Platform.Maui)
            {
				success = EdubaiLocalStorage.Instance.Set("edubaiauthstatetoken", encryptedToken, PlatformInfo);
            }
            else
            {
                success = await EdubaiLocalStorage.Instance.SetAsync("edubaiauthstatetoken", encryptedToken, PlatformInfo, LocalStorage);
            }

            return success;
        }



        #region Static Helper Methods
        //------------------------------------------------------------------------------------------
        //------------------------------------- Helper Methods--------------------------------------
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the default Jwt for the System user
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetDefaultAuthenticationStateJwt()
        {
            // Code to generate the encrypted System Jwt
            // Needs the Encryption API controller to allow anonymous calls
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "System"),
                new Claim(ClaimTypes.Role, "System")
            };

            EdubaiAESEncryptionController aes = new EdubaiAESEncryptionController(PlatformInfo, Http, this);
            string jwt = await aes.Encrypt(await GenerateJwtFromClaims(claims, null));

            return jwt;
        }

        /// <summary>
        /// Parse the claims from a valid Jwt
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Claim>> ParseClaimsFromJwt(string jwt)
		{
			var payload = jwt.Split('.')[1];
			var jsonBytes = ParseBase64WithoutPadding(payload);
			var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
			return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
		}

        /// <summary>
        /// Create a Jwt from the given claims and a fixed expiration time
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        private async Task<string> GenerateJwtFromClaims(IEnumerable<Claim> claims, string passwordHash)
        {
            List<Claim> claimsList = claims.ToList();
            DateTime expires = DateTime.UtcNow.AddHours(2);

            EdubaiAESEncryptionController aes = new EdubaiAESEncryptionController(PlatformInfo, Http, this);

            return await aes.SignedJwt(claimsList, expires, passwordHash);       
        }

        /// <summary>
        /// Get a valid Jwt or the default Jwt (System) if the given Jwt is invalid or expired
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<string> GetValidOrDefault(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                // Decrypt the JWT
                EdubaiAESEncryptionController aes = new EdubaiAESEncryptionController(PlatformInfo, Http, this);
                string decryptedToken = await aes.Decrypt(token);

                //check expiration
                List<Claim> claims = ParseClaimsFromJwt(decryptedToken).Result.ToList();

                DateTime expiration = DateTime.UnixEpoch.AddSeconds(int.Parse(claims.FirstOrDefault(c => c.Type == "exp").Value));
                if (expiration > DateTime.UtcNow)
                {
                    return token;
                }
            }

            return await GetDefaultAuthenticationStateJwt();
        }   

        /// <summary>
        /// Parse a base64 string without padding
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        private static byte[] ParseBase64WithoutPadding(string base64)
		{
			switch (base64.Length % 4)
			{
				case 2: base64 += "=="; break;
				case 3: base64 += "="; break;
			}
			return Convert.FromBase64String(base64);
		}

        #endregion
    }
}