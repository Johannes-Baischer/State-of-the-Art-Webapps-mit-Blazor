using Microsoft.AspNetCore.Components;
using SharedComponents.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using SharedComponents.Development;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using SharedComponents.Services;

namespace SharedComponents.PostgreSQL
{

    /// <summary>
    /// Mirrors API Controller for Serverconnections
    /// Calls Services directly for self hosted platforms
    /// Calls API Controller for Web hosted platforms (BlazorWasm)
    /// </summary>
    public class UserCredentialsController : ComponentBase
    {
        private const string ROUTEASAPICONTROLLER = "internal/UserCredentials";
        private IPlatformInfo? PlatformInfo { get; set; } = default!;
        private HttpClient? Http { get; set; } = default!;
        private AuthenticationStateProvider? AuthenticationStateProvider { get; set; } = default!;



        /// <summary>
        /// Contructor Injection because [inject] only in cs files connected to a razor page
        /// </summary>
        /// <param name="platformInfo">Current Platform
        /// Nullable for calls with no access to PlatformInfo (e.g. in BlazorWasmHost)</param>
        /// <param name="http">HttpClient, most likely from dependency injection
        /// Nullable for calls with no access to PlatformInfo (e.g. in BlazorWasmHost)</param>
        /// <param name="authenticationStateProvider">AuthenticationStateProvider, used to get permissions of current user</param>
        public UserCredentialsController(IPlatformInfo? platformInfo, HttpClient? http, AuthenticationStateProvider authenticationStateProvider)
        {
            PlatformInfo = platformInfo;
            Http = http;
            AuthenticationStateProvider = authenticationStateProvider;
        }



        /// <summary>
        /// Creates a new UserCredential in the Database
        /// </summary>
        /// <param name="newUserCredential">Usercredentials to be added</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns></returns>
        public async Task<UserCredential?> Insert(UserCredential newUserCredential, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo!.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { newUserCredential };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/Insert", content);


                //------------------Response------------------
                // Check if the response was successful
                UserCredential? userCredential;
                //Successfully called by the server
                if ((userCredential = await response.Content.ReadFromJsonAsync<UserCredential?>()) == null)
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(4) HTTP Error! Internal Server Error", "UserCredentials Controller");
                    return null;
                }

                return userCredential;
            }
            #endregion

            try
            {
                bool userExists = await CheckUserExists(newUserCredential.Email, callerIsServer);

                //DB Call
                using (var db = new EdubaiContext())
                {
                    if (userExists)
                    {
                        EduBaiMessaging.ConsoleLog("User with that Email already exists!", "UserCredentials Controller");
                        EduBaiMessaging.Throw(new HttpRequestException("User with that Email already exists!", null, HttpStatusCode.Conflict), "UserCredentials Controller", callerIsServer);
                        return null;
                    }
                    else
                    {
                        //Insert
                        db.UserCredentials.Add(newUserCredential);
                    }
                    db.SaveChanges();
                }

                return newUserCredential;
            }
            catch (Exception ex)
            {
                //TODO: Error Toast
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/UserCredentials Controller");
                EduBaiMessaging.Throw(ex, "UserCredentials Controller", callerIsServer);
                return null;
            }
        }



        /// <summary>
        /// Creates a new UserCredential in the Database
        /// </summary>
        /// <param name="newUserCredential">Usercredentials to be added</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns></returns>
        public async Task<UserCredential?> Upsert(UserCredential newUserCredential, bool callerIsServer = false)
        {
            #region Additional Security Checks

            if (AuthenticationStateProvider == null)
            {
                // Special case, when this method is called from the server
                // In this case, the AuthenticationStateProvider is not available, but all checks have already been done
                
                string token = Http.DefaultRequestHeaders.Authorization.ToString().Replace("Bearer ", "");
                IEnumerable<Claim> claims = await EdubaiAuthStateProvider.ParseClaimsFromJwt(token);

                if (claims == null)
                {
                    EduBaiMessaging.ConsoleLog("No claims found in JWT!", "UserCredentials Controller");
                    EduBaiMessaging.Throw(new HttpRequestException("No claims found in JWT!", null, HttpStatusCode.Unauthorized), "UserCredentials Controller", callerIsServer);
                    return null;
                }

                string currentEmail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                if (currentEmail != newUserCredential.Email)
                {
                    EduBaiMessaging.ConsoleLog($"User {currentEmail} is unauthorized to change data of {newUserCredential.Email}!", "UserCredentials Controller");
                    EduBaiMessaging.Throw(new HttpRequestException("Unauthorized to change this users data!", null, HttpStatusCode.Unauthorized), "UserCredentials Controller", callerIsServer);
                    return null;
                }

            }
            else
            {
                // Check if the user is trying to change other users data
                string currentEmail = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                if (currentEmail != newUserCredential.Email)
                {
                    EduBaiMessaging.ConsoleLog($"User {currentEmail} is unauthorized to change data of {newUserCredential.Email}!", "UserCredentials Controller");
                    EduBaiMessaging.Throw(new HttpRequestException("Unauthorized to change this users data!", null, HttpStatusCode.Unauthorized), "UserCredentials Controller", callerIsServer);
                    return null;
                }
            }            

            #endregion

            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo!.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { newUserCredential };

                var json = JsonSerializer.Serialize(new object[]{ services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/Upsert", content);


                //------------------Response------------------
                // Check if the response was successful
                UserCredential? userCredential;
                //Successfully called by the server
                if ((userCredential = await response.Content.ReadFromJsonAsync<UserCredential?>()) == null)
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(3) HTTP Error! Internal Server Error", "UserCredentials Controller");
                    return null;
                }

                return userCredential;
            }
            #endregion

            try
            {
                bool userExists = await CheckUserExists(newUserCredential.Email, callerIsServer);

                //DB Call
                using (var db = new EdubaiContext())
                {
                    if (userExists)
                    {
                        //Update
                        db.UserCredentials.Update(newUserCredential);
                    }
                    else
                    {
                        //Insert
                        db.UserCredentials.Add(newUserCredential);
                    }
                    db.SaveChanges();
                }

                return newUserCredential;
            }
            catch (Exception ex)
            {
                //TODO: Error Toast
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/UserCredentials Controller");
                EduBaiMessaging.Throw(ex, "UserCredentials Controller", callerIsServer);
                return null;
            }
        }

        

        /// <summary>
        /// Gets a UserCredential from the Database
        /// </summary>
        /// <param name="Email">User Email</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns>UserCredentials if Email and Password are correct, null otherwise</returns>
        public async Task<UserCredential?> Read(string Email, string hashedGivenPassword, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                string[] args = { Email, hashedGivenPassword };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/Read", content);

                UserCredential? userCredential = default;
                //Successfully called by the server
                try
                {
                    userCredential = await response.Content.ReadFromJsonAsync<UserCredential?>();
                }
                catch
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(1) HTTP Error! Internal Server Error", "UserCredentials Controller");
                    return null;
                }

                return userCredential;
            }
            #endregion

            UserCredential? uc;
            try
            {
                //DB Call
                using (var db = new EdubaiContext())
                {
                    uc = db.UserCredentials.Where(u => u.Email == Email).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/UserCredentials Controller");
                EduBaiMessaging.Throw(ex, "UserCredentials Controller", callerIsServer);
                return null;
            }

            if (uc == null)
            {
                //Email not found
                EduBaiMessaging.ConsoleLog($"(2) User: {Email} not found!", "UserCredentials Controller");
                EduBaiMessaging.Throw(new HttpRequestException($"(2) User: {Email} not found!", null, HttpStatusCode.NotFound), "UserCredentials Controller", callerIsServer);
                return null;
            }

            if (uc.PasswordHash != hashedGivenPassword)
            {
                //Password incorrect
                EduBaiMessaging.ConsoleLog($"(3) Password incorrect for User: {Email}!", "UserCredentials Controller");
                EduBaiMessaging.Throw(new HttpRequestException($"(3) Password incorrect for User: {Email}!", null, HttpStatusCode.Unauthorized), "UserCredentials Controller", callerIsServer);
                return null;
            }

            //is not null at this point
            return uc!;
        }


        /// <summary>
        /// Gets the password salt for a given user in the database
        /// </summary>
        /// <param name="Email">User Email</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns>string if Email and Password are correct, null otherwise</returns>
        public async Task<string?> ReadSalt(string Email, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { Email };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/ReadSalt", content);

                string? salt = default;
                //Successfully called by the server
                try
                {
                    salt = await response.Content.ReadFromJsonAsync<string?>();
                }
                catch
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(1) HTTP Error! Internal Server Error", "UserCredentials Controller");
                    return null;
                }

                return salt;
            }
            #endregion

            UserCredential? uc;
            try
            {
                //DB Call
                using (var db = new EdubaiContext())
                {
                    uc = db.UserCredentials
                        .Select(u => new UserCredential
                        {
                            Email = u.Email,
                            PasswordSalt = u.PasswordSalt
                        })
                        .Where(u => u.Email == Email).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/UserCredentials Controller");
                EduBaiMessaging.Throw(ex, "UserCredentials Controller", callerIsServer);
                return null;
            }

            if (uc == null)
            {
                //Email not found
                EduBaiMessaging.ConsoleLog($"(2) User: {Email} not found!", "UserCredentials Controller");
                EduBaiMessaging.Throw(new HttpRequestException($"(2) User: {Email} not found!", null, HttpStatusCode.NotFound), "UserCredentials Controller", callerIsServer);
                return null;
            }

            //is not null at this point
            return uc.PasswordSalt!;
        }


        /// <summary>
        /// Checks if a user exists in the database
        /// </summary>
        /// <param name="Email">User Email</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns>true if user with given email exists in database, false otherwise</returns>
        public async Task<bool> CheckUserExists(string Email, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { Email };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/CheckUserExists", content);

                bool userExists = false;
                //Successfully called by the server
                try
                {
                    userExists = await response.Content.ReadFromJsonAsync<bool>();
                }
                catch
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(1) HTTP Error! Internal Server Error", "UserCredentials Controller");
                    return false;
                }

                return userExists;
            }
            #endregion

            UserCredential? uc;
            try
            {
                //DB Call
                using (var db = new EdubaiContext())
                {
                    uc = db.UserCredentials
                        .Select(u => new UserCredential
                        {
                            Email = u.Email
                        })
                        .Where(u => u.Email == Email).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/UserCredentials Controller");
                EduBaiMessaging.Throw(ex, "UserCredentials Controller", callerIsServer);
                return false;
            }

            if (uc == null)
            {
                //Email not found
                EduBaiMessaging.ConsoleLog($"(2) User: {Email} not found!", "UserCredentials Controller");
                return false;
            }

            //is not null at this point
            return true;
        }

        /// <summary>
        /// Checks if the provided PasswordResetToken matches the one in the database
        /// </summary>
        /// <param name="Email">User Email</param>
        /// <param name="PasswordResetToken">PasswordResetToken to be checked against the database</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns>true if user with given email exists in database, false otherwise</returns>
        public async Task<bool> CheckPasswordResetTokenMatch(string Email, string PasswordResetToken, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { Email, PasswordResetToken };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/CheckPasswordResetTokenMatch", content);

                bool isOk = false;
                //Successfully called by the server
                try
                {
                    isOk = await response.Content.ReadFromJsonAsync<bool>();
                }
                catch
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(1) HTTP Error! Internal Server Error", "UserCredentials Controller");
                    return false;
                }

                return isOk;
            }
            #endregion

            UserCredential? uc;
            try
            {
                //DB Call
                using (var db = new EdubaiContext())
                {
                    uc = db.UserCredentials
                        .Select(u => new UserCredential
                        {
                            Email = u.Email,
                            PasswordResetToken = u.PasswordResetToken
                        })
                        .Where(u => u.Email == Email).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/UserCredentials Controller");
                EduBaiMessaging.Throw(ex, "UserCredentials Controller", callerIsServer);
                return false;
            }

            if (uc == null)
            {
                //Email not found
                EduBaiMessaging.ConsoleLog($"(2) User: {Email} not found!", "UserCredentials Controller");
                return false;
            }

            if (uc.PasswordResetToken != PasswordResetToken)
            {
                //Token does not match
                EduBaiMessaging.ConsoleLog($"(3) PasswordResetToken does not match!", "UserCredentials Controller");
                return false;
            }

            //passed every check
            return true;
        }

        /// <summary>
        /// Checks if the provided PasswordResetToken matches the one in the database
        /// </summary>
        /// <param name="Email">User Email</param>
        /// <param name="PasswordResetToken">PasswordResetToken to be checked against the database</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns>true if user with given email exists in database, false otherwise</returns>
        public async Task<bool> ResetPassword(string Email, string NewPasswordHash, string PasswordResetToken, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------


                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { Email, NewPasswordHash, PasswordResetToken };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/ResetPassword", content);

                bool isOk = false;
                //Successfully called by the server
                try
                {
                    isOk = await response.Content.ReadFromJsonAsync<bool>();
                }
                catch
                {
                    // Handle the error response
                    //TODO: Error Toast
                    EduBaiMessaging.ConsoleLog($"(1) HTTP Error! Internal Server Error", "UserCredentials Controller");
                    return false;
                }

                return isOk;
            }
            #endregion

            UserCredential? uc;
            try
            {
                bool matches = await CheckPasswordResetTokenMatch(Email, PasswordResetToken, callerIsServer);

                if (matches == false)
                {
                    EduBaiMessaging.ConsoleLog($"(3) PasswordResetToken: {PasswordResetToken} does not match the one in the database!", "UserCredentials Controller");
                    EduBaiMessaging.Throw(new HttpRequestException($"(3) PasswordResetToken: {PasswordResetToken} does not match the one in the database!", null, HttpStatusCode.BadRequest), "UserCredentials Controller", callerIsServer);
                    return false;
                }
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog(ex.ToString(), "Exception/UserCredentials Controller");
                EduBaiMessaging.Throw(ex, "UserCredentials Controller", callerIsServer);
                return false;
            }

            //Token Matches and User exists
            //DB Call
            using (var db = new EdubaiContext())
            {
                uc = db.UserCredentials.Where(u => u.Email == Email).FirstOrDefault();
                uc.PasswordHash = NewPasswordHash;
                uc.PasswordResetToken = null;

                db.SaveChanges();
            }

            //passed every check
            return true;
        }

        #region Static Helper Methods
        //------------------------------------------------------------------------------------------
        //-------------------------------Static Helper Methods--------------------------------------
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// Generates a random salt for more secure password hashing
        /// </summary>
        /// <returns>Random Hash as String</returns>
        public static string GenerateSalt()
		{
			// Salt
			Random rng = new Random(DateTime.Now.Millisecond);
			string randomNumber = rng.Next(100000, 1000000).ToString();
			byte[] saltHashBytes = SHA256.HashData(Encoding.ASCII.GetBytes(randomNumber));
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in saltHashBytes)
			{
				stringBuilder.AppendFormat("{0:X2}", b);
			}
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Hashes a password with a given salt
		/// </summary>
		/// <param name="password">Original Password to be hashed (Clear Text)</param>
		/// <param name="salt">Salt that was generated when the user was created (Hash)</param>
		/// <returns>Hashed Password with Salt as String</returns>
		public static string HashPasswordWithSalt(string password, string salt)
		{
			byte[] passwordHashBytes = SHA256.HashData(Encoding.ASCII.GetBytes(password + salt));
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in passwordHashBytes)
			{
				stringBuilder.AppendFormat("{0:X2}", b);
			}
			return stringBuilder.ToString();
		}
		#endregion
	}
}