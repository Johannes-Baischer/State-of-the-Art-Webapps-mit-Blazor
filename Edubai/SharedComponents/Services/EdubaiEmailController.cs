using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SharedComponents.Development;
using SharedComponents.PostgreSQL;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using SharedComponents.Configuration;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Maui.ApplicationModel.Communication;

namespace SharedComponents.Services
{
    public class EdubaiEmailController
    {
        private const string ROUTEASAPICONTROLLER = "internal/EdubaiEmail";
        private string EdubaiMail { get; set; } = "Johannes.Baischer@gmail.com";
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
        public EdubaiEmailController(IPlatformInfo? platformInfo, HttpClient? http, AuthenticationStateProvider authenticationStateProvider)
        {
            PlatformInfo = platformInfo;
            Http = http;
            AuthenticationStateProvider = authenticationStateProvider;
        }

        /// <summary>
        /// Sends an email from the EduBai Service to the specified email address
        /// </summary>
        /// <param name="toEmail">Recipiant Email</param>
        /// <param name="subject">Subject of the Email</param>
        /// <param name="message">HTML formated Email body</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Plattformkompatibilität überprüfen", Justification = "Call gets redirected to Server")]
        public async Task<bool> Send(string toEmail, string subject, string message, bool callerIsServer = false)
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
                    EduBaiMessaging.ConsoleLog("No claims found in JWT!", "Email Controller");
                    EduBaiMessaging.Throw(new HttpRequestException("No claims found in JWT!", null, HttpStatusCode.Unauthorized), "Email Controller", callerIsServer);
                    return false;
                }

                string currentEmail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                if (currentEmail != toEmail)
                {
                    EduBaiMessaging.ConsoleLog($"User {currentEmail} is unauthorized to send emails to {toEmail}!", "Email Controller");
                    EduBaiMessaging.Throw(new HttpRequestException("Unauthorized to send emails to this User!", null, HttpStatusCode.Unauthorized), "Email Controller", callerIsServer);
                    return false;
                }
            }
            else
            {
                // Check if the user is trying to change other users data
                string currentEmail = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                if (currentEmail != toEmail)
                {
                    EduBaiMessaging.ConsoleLog($"User {currentEmail} is unauthorized to send emails to {toEmail}!", "Email Controller");
                    EduBaiMessaging.Throw(new HttpRequestException("Unauthorized to send emails to this User!", null, HttpStatusCode.Unauthorized), "Email Controller", callerIsServer);
                    return false;
                }
            }

            #endregion

            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------

                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { toEmail, subject, message };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/Send", content);

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
                    EduBaiMessaging.ConsoleLog($"(1) Error reading response! Internal Server Error", "Email Controller");
                    return false;
                }

                return isOk;
            }
            #endregion

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(EdubaiMail, GetEdubaiMailPassword())
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(EdubaiMail),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog($"(2) SMTP Error! {ex.Message}", "Email Controller");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies the email in the database, if the token is correct
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="EmailVerificationToken"></param>
        /// <param name="callerIsServer"></param>
        /// <returns></returns>
        public async Task<bool> VerifyEmail(string Email, string EmailVerificationToken, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------

                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { Email, EmailVerificationToken };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/VerifyEmail", content);

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
                    EduBaiMessaging.ConsoleLog($"(1) Error reading response! Internal Server Error", "Email Controller");
                    return false;
                }

                return isOk;
            }
            #endregion

            //DB Call
            using (var db = new EdubaiContext())
            {
                UserCredential uc = db.UserCredentials.Where(u => u.Email == Email).FirstOrDefault();

                if (uc != null)
                {
                    if(uc.EmailVerificationToken == EmailVerificationToken)
                    {
                        uc.EmailIsVerified = true;
                        db.UserCredentials.Update(uc);
                    }
                    else
                    {
                        EduBaiMessaging.ConsoleLog($"Email Validation Token mismatch for User {Email}!", "Email Controller");
                        EduBaiMessaging.Throw(new HttpRequestException("Email Validation Token mismatch", null, HttpStatusCode.BadRequest), "Email Controller", callerIsServer);
                        return false;
                    }
                }
                else
                {
                    EduBaiMessaging.ConsoleLog($"User {Email} not found!", "Email Controller");
                    EduBaiMessaging.Throw(new HttpRequestException("User not found", null, HttpStatusCode.NotFound), "Email Controller", callerIsServer);
                    return false;
                }

                db.SaveChanges();
            }

            return true;
        }

        /// <summary>
        /// Sends an email from the EduBai Service to the specified email address with a password reset link
        /// The Link contains a token which is saved to be checked against the database later
        /// </summary>
        /// <param name="toEmail">Recipiant Email</param>
        /// <param name="callerIsServer">Flag for dedicated call from Server (important for BlazorWasm/Web only!)</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Plattformkompatibilität überprüfen", Justification = "Call gets redirected to Server")]
        public async Task<bool> SendPasswordReset(string toEmail, bool callerIsServer = false)
        {
            #region Redirect to Server
            if (callerIsServer == false && PlatformInfo?.Platform == Platform.Web)
            {
                //-----------------------------------------------------
                //Redirect to Server if request comming from BlazorWasm
                //-----------------------------------------------------

                //------------------Convert the user credential to a JSON string------------------
                object[] services = { PlatformInfo, Http, AuthenticationStateProvider };
                object[] args = { toEmail };

                var json = JsonSerializer.Serialize(new object[] { services, args });


                //------------------Set up the HttpClient------------------
                Http!.DefaultRequestHeaders.Accept.Clear();
                Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                //------------------POST------------------
                // Create the POST request content
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await Http.PostAsync(ROUTEASAPICONTROLLER + "/SendPasswordReset", content);

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
                    EduBaiMessaging.ConsoleLog($"(3) Error reading response! Internal Server Error", "Email Controller");
                    return false;
                }

                return isOk;
            }
            #endregion

            string PasswordResetToken = UserCredentialsController.GenerateSalt();
            //DB Call
            using (var db = new EdubaiContext())
            {
                UserCredential uc = db.UserCredentials.Where(u => u.Email == toEmail).FirstOrDefault();

                if (uc != null)
                {
                    uc.PasswordResetToken = PasswordResetToken;
                    db.UserCredentials.Update(uc);
                }
                else
                {
                    EduBaiMessaging.ConsoleLog($"User {toEmail} not found!", "Email Controller");
                    EduBaiMessaging.Throw(new HttpRequestException("User not found", null, HttpStatusCode.NotFound), "Email Controller", callerIsServer);
                    return false;
                }

                db.SaveChanges();
            }

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(EdubaiMail, GetEdubaiMailPassword())
            };

            if (PasswordResetToken.All(c => char.IsLetterOrDigit(c)) == false)
            {
                //simple check for malicious code (html tags)
                EduBaiMessaging.ConsoleLog($"(4) Token contains invalid characters! {PasswordResetToken}", "Email Controller");
                EduBaiMessaging.Throw(new HttpRequestException("Token contains invalid characters!", null, HttpStatusCode.BadRequest));
                return false;
            }

            string resetLink = "<a href=\"" + Http.BaseAddress + "forgotpassword?Email=" + toEmail + "&PasswordResetToken=" + PasswordResetToken + "\"> Click here to reset your Password </a>";
            var mailMessage = new MailMessage
            {
                // todo figure out localization
                From = new MailAddress(EdubaiMail),
                Subject = "Edubai - Password Reset",
                Body = "If you requested a Password Reset for your Account: <br />" +
                "Please continue by clicking on the following link: <br />" +
                resetLink,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                EduBaiMessaging.ConsoleLog($"(4) SMTP Error! {ex.Message}", "Email Controller");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the Edubai Mail Password from the Configuration
        /// Implemented as a static method, to access App Assembly
        /// </summary>
        /// <returns>EmailPassword read from the user secrets</returns>
        private static string GetEdubaiMailPassword()
        {
            IConfiguration config;
            string password = "null";

            try
            {
                //get secrets from user secrets
                config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("secrets.json")
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                    .Build();

                password = config?["EdubaiMailPassword"] ?? "null";
            }
            catch
            {
                //TODO: Error Toast
                EduBaiMessaging.ConsoleLog($"(1) Configuration Error! Internal Server Error", "Email Controller");
            }

            return password;
        }
    }
}
