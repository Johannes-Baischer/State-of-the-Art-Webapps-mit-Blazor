using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using SharedComponents.Configuration;
using SharedComponents.Development;
using SharedComponents.PostgreSQL;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace SharedComponents.Services
{
    public class EdubaiAESEncryptionController
    {
        private const string ROUTEASAPICONTROLLER = "internal/EdubaiAESEncryption";

        protected IPlatformInfo? PlatformInfo { get; set; } = default!;
        protected HttpClient? Http { get; set; } = default!;
        protected AuthenticationStateProvider? AuthenticationStateProvider { get; set; } = default!;

        /// <summary>
        /// Contructor Injection because [inject] only in cs files connected to a razor page
        /// </summary>
        /// <param name="platformInfo">Current Platform
        /// Nullable for calls with no access to PlatformInfo (e.g. in BlazorWasmHost)</param>
        /// <param name="http">HttpClient, most likely from dependency injection
        /// Nullable for calls with no access to PlatformInfo (e.g. in BlazorWasmHost)</param>
        public EdubaiAESEncryptionController(IPlatformInfo? platformInfo, HttpClient? http, AuthenticationStateProvider? authenticationStateProvider)
        {
            PlatformInfo = platformInfo;
            Http = http;
            AuthenticationStateProvider = authenticationStateProvider;
        }

        /// <summary>
        /// Symetrically Encrypt the plaintext with the given key
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key">if no key is supplied, the global Edubai Encryption key will be used</param>
        /// <param name="callerIsServer">If the caller is the server, the request will not be redirected to the server</param>
        /// <returns>Encrypted text</returns>
        public async Task<string?> Encrypt(string plainText, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { plainText };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/Encrypt", content);

                string? cipher = null;
                //Successfully called by the server
                try
                {
                    cipher = await response.Content.ReadFromJsonAsync<string?>();
                }
                catch (Exception e)
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(1) Error reading response! Internal Server Error: " + e, "EdubaiAESEncryptionController");
                    return null;
                }

                return cipher;
            }
            #endregion

            // Encrypt the plain text
            string key = GetValueFromConfig("EdubaiJwtEncryptionKey");

            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16];
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Symmetrically decrypt the given ciphertext
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="callerIsServer">If the caller is the server, the request will not be redirected to the server</param>
        /// <returns>plain text</returns>
        public async Task<string?> Decrypt(string cipherText, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------

                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { cipherText };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/Decrypt", content);

                string? plain = null;
                //Successfully called by the server
                try
                {
                    plain = await response.Content.ReadFromJsonAsync<string?>();
                }
                catch (Exception e)
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(2) Error reading response! Internal Server Error: " + e, "EdubaiAESEncryptionController");
                    return null;
                }

                return plain;
            }
            #endregion

            // Decrypt the cipher text
            string key = GetValueFromConfig("EdubaiJwtEncryptionKey");

            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16];
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }


        /// <summary>
        /// Generate a signed JWT token with the given claims
        /// </summary>
        /// <param name="claimsList">List of Claims to be made in the Jwt</param>
        /// <param name="expires">Expiration date of the token</param>
        /// <param name="passwordHash">Password given through the login form, to validate the legitimacy of the claims</param>
        /// <param name="callerIsServer">If the caller is the server, the request will not be redirected to the server</param>
        /// <returns></returns>
        public async Task<string?> SignedJwt(List<Claim> claimsList, DateTime expires, string passwordHash, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { claimsList, expires, passwordHash };

                var json = JsonSerializer.Serialize(new object[] { services, args });

                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/SignedJwt", content);

                string? jwt = null;
                //Successfully called by the server
                try
                {
                    jwt = await response.Content.ReadFromJsonAsync<string?>();
                }
                catch(Exception e)
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(3) Error reading response! Internal Server Error: " + e, "EdubaiAESEncryptionController");
                    return null;
                }

                return jwt;
            }
            #endregion


            //Check for special case of System claims
            if (claimsList.Count == 2) 
            {
                if (claimsList.Find(c => c.Type == ClaimTypes.Name).Value == "System" && claimsList.Find(c => c.Type == ClaimTypes.Role).Value == "System")
                {
                    return GenerateJwt(claimsList, expires);
                }
            }

            // Get the user credential from the database
            string Email = claimsList.Find(c => c.Type == ClaimTypes.Name).Value;
            UserCredential? userCredential = await new UserCredentialsController(PlatformInfo, Http, AuthenticationStateProvider).Read(Email, passwordHash, callerIsServer);
            if (userCredential == null)
            {
                //TODO: Show error Toast
                EduBaiMessaging.ConsoleLog("Signing Jwt failed! User not found or password mismatch!", "EdubaiAESEncryptionController");
                EduBaiMessaging.Throw(new HttpRequestException("User not found or password mismatch!", null, HttpStatusCode.Unauthorized), "EdubaiAESEncryptionController", callerIsServer);
            }

            // Check for matching claims with the user credential
            foreach (Claim claim in claimsList)
            {
                switch (claim.Type)
                {
                    case ClaimTypes.Name:
                        if (claim.Value != userCredential.Email)
                        {
                            EduBaiMessaging.Throw(new HttpRequestException("The given Email did not match the user credential!", null, HttpStatusCode.Unauthorized), "EdubaiAESEncryption Controller", callerIsServer);
                            return null;
                        }
                        break;
                    case ClaimTypes.Role:
                        if (claim.Value != userCredential.Role)
                        {
                            EduBaiMessaging.Throw(new HttpRequestException("The given role did not match the user credential!", null, HttpStatusCode.Unauthorized), "EdubaiAESEncryption Controller", callerIsServer);
                            return null;
                        }
                        break;
                    default:
                        break;
                }
            }


            return GenerateJwt(claimsList, expires);
        }

        /// <summary>
        /// Builds a JWT token from the given claims list and expiration date
        /// </summary>
        /// <param name="claimsList"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        private string GenerateJwt(List<Claim> claimsList, DateTime expires)
        {
            string key = GetValueFromConfig("EdibaiBearerSigningKey");
            SymmetricSecurityKey secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials signcred = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claimsList,
                signingCredentials: signcred,
                expires: expires);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
		/// Reads the config, and returns the value for the given key, can only be called from the server
		/// </summary>
		/// <returns>(String or null if not found)</returns>
		private string GetValueFromConfig(string key)
        {
            IConfiguration config;
            try
            {
                //try to find appsettings.json file
                config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("secrets.json")
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                    .Build();
            }
            catch
            {
                config = new ConfigurationBuilder()
                .Build();
            }

            if (config == null)
                EduBaiMessaging.ConsoleLog("Config null", "EdubaiConfigController");

            if (config?[key] == null)
                EduBaiMessaging.ConsoleLog(key + " not found in config", "EdubaiConfigController");

            return config?[key];
        }
    }
}
