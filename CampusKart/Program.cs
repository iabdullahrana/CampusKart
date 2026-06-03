using CampusKart.Client.Pages;
using CampusKart.Components;
using CampusKart.Components.Account;
using CampusKart.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

namespace CampusKart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents(options =>
                {
                    options.DetailedErrors = true;
                })
                .AddHubOptions(options =>
                {
                    options.MaximumReceiveMessageSize = 50 * 1024 * 1024; // 50MB limit for image uploads/streaming
                })
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 50 * 1024 * 1024; // 50MB limit
            });

            builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(options =>
            {
                options.MaximumReceiveMessageSize = 50 * 1024 * 1024; // 50MB limit
            });

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
                    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey("picture", "picture");
                    options.Events.OnTicketReceived = context =>
                    {
                        var principal = context.Principal;
                        var email = principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                                    ?? principal?.FindFirst("email")?.Value
                                    ?? principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                        if (string.IsNullOrEmpty(email))
                        {
                            context.Fail("Email address not found in Google account claims.");
                        }
                        return Task.CompletedTask;
                    };
                });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
            });

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            // Register Cloudinary account configuration conditionally
            var cloudinarySection = builder.Configuration.GetSection("Cloudinary");
            var cloudName = cloudinarySection["CloudName"];
            var apiKey = cloudinarySection["ApiKey"];
            var apiSecret = cloudinarySection["ApiSecret"];

            // Bypass local SSL certificate validation errors globally for all HttpClient calls in this process (including inside the Cloudinary SDK)
            System.Net.ServicePointManager.ServerCertificateValidationCallback = 
                (sender, certificate, chain, sslPolicyErrors) => true;

            if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret) &&
                cloudName != "your-cloud-name" && apiKey != "your-api-key" && apiSecret != "your-api-secret")
            {
                var account = new CloudinaryDotNet.Account(cloudName, apiKey, apiSecret);
                var cloudinary = new CloudinaryDotNet.Cloudinary(account);

                // Surgical injection of SSL-bypassing HttpClient into Cloudinary SDK Client to solve SSL handshake failures on localhost/network
                try
                {
                    var handler = new System.Net.Http.HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                    };
                    var httpClient = new System.Net.Http.HttpClient(handler);

                    var apiProp = typeof(CloudinaryDotNet.Cloudinary).GetProperty("Api", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (apiProp != null)
                    {
                        var apiObj = apiProp.GetValue(cloudinary);
                        if (apiObj != null)
                        {
                            var clientField = typeof(CloudinaryDotNet.Api).GetField("Client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                            if (clientField != null)
                            {
                                clientField.SetValue(apiObj, httpClient);
                                Console.WriteLine(">>> [DI Configuration] Successfully injected SSL-bypassing HttpClient into Cloudinary SDK Client!");
                            }
                            else
                            {
                                Console.WriteLine(">>> [DI Configuration] Client field not found!");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">>> [DI Configuration] Error injecting SSL-bypassing HttpClient into Cloudinary: {ex.Message}");
                }

                builder.Services.AddSingleton(cloudinary);
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.MapGet("/test-cloudinary-connectivity", async (CloudinaryDotNet.Cloudinary cloudinary) =>
            {
                try
                {
                    var testParams = new CloudinaryDotNet.Actions.ImageUploadParams()
                    {
                        File = new CloudinaryDotNet.FileDescription("test.jpg", new System.IO.MemoryStream(new byte[] { 1, 2, 3, 4 })),
                        Folder = "test_diagnostics"
                    };
                    var result = await cloudinary.UploadAsync(testParams);
                    return Results.Json(new 
                    {
                        Success = result.Error == null,
                        Error = result.Error?.Message,
                        StatusCode = result.StatusCode,
                        Url = result.SecureUrl?.ToString()
                    });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Success = false, Error = ex.ToString() });
                }
            });

            app.Run();
        }
    }
}
