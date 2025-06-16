using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.Configuration;
using SharedComponents.Policies;
using SharedComponents.Services;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add configuration files
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile("secrets.json")
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

//Authorization and Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(builder.Configuration["EdibaiBearerSigningKey"]));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(hmac.Key)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmailIsVerified", policy =>
    {
        policy.AddRequirements(new EmailVerifiedRequirement("EmailIsVerified", new string[] { true.ToString() }));
    });
});



// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddOptions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles(new StaticFileOptions
{
    // Adds static file serving for all file types (e.g. .h5p)
    ServeUnknownFileTypes = true
});
app.UseRequestLocalization(LocalizationInfo.Instance.GetRequestLocalizationOptions());

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

//fix for CORS Problems
//app.UseCors("AllowAllOrigins");
//app.UseCors(cors => cors
//                .AllowAnyMethod()
//                .AllowAnyHeader()
//                .SetIsOriginAllowed(origin => true)
//                .AllowCredentials()
//            );

app.Run();
