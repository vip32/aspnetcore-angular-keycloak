
namespace WebApp
{
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.SpaServices.AngularCli;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using NSwag;
    using NSwag.AspNetCore;
    using NSwag.Generation.AspNetCore;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Security;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddAuthentication(options => this.ConfigureAuthentication(options))
                .AddJwtBearer(options => this.ConfigureJwtBearer(options));
            services.AddAuthorization();

            services.AddApiVersioning(options => this.ConfigureApiVersioning(options));
            services.AddVersionedApiExplorer(options => options.SubstituteApiVersionInUrl = true);

            services.AddProblemDetails(this.ConfigureProblemDetails);

            services.AddOpenApiDocument(document => this.ConfigureOpenApiDocument(this.Configuration, document));

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseOpenApi();
            app.UseSwaggerUi3(settings =>
            {
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    ClientId = this.Configuration["Oidc:ClientId"],
                    AppName = this.GetType().Namespace,
                };
                settings.SwaggerRoutes.Add(new SwaggerUi3Route(this.GetType().Namespace, "swagger/v1/swagger.json"));
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                    //spa.UseProxyToSpaDevelopmentServer("http://localhost:4200") // use 'ng serve' seperatly
                }
            });
        }

        private void ConfigureAuthentication(AuthenticationOptions options)
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }

        private void ConfigureJwtBearer(JwtBearerOptions options)
        {
            options.Authority = this.Configuration["Oidc:Authority"];
            options.Audience = this.Configuration["Oidc:ClientId"];
            options.IncludeErrorDetails = true;
            options.RequireHttpsMetadata = false; // only DEV should use non https
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "groups",
                ValidateAudience = false,
                //ValidAudiences = new[] { "master-realm", "account" },
                ValidateIssuer = true,
                ValidIssuer = this.Configuration["Oidc:Authority"],
                ValidateLifetime = false
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = (ctx) =>
                {
                    // add some claim when authenticated
                    string email = ctx.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                    var claims = new List<Claim>
                    {
                        new Claim("DummyClaim", email ?? "unknown")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims);
                    ctx.Principal.AddIdentity(claimsIdentity);

                    return Task.CompletedTask;
                }
            };
        }

        private void ConfigureApiVersioning(ApiVersioningOptions options)
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        }

        private void ConfigureOpenApiDocument(IConfiguration configuration, AspNetCoreOpenApiDocumentGeneratorSettings settings)
        {
            settings.DocumentName = "v1";
            settings.Version = "v1";
            settings.Title = this.GetType().Namespace;
            settings.AddSecurity(
                "bearer",
                Enumerable.Empty<string>(),
                new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Flow = OpenApiOAuth2Flow.Implicit,
                    Description = "Oidc Authentication",
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = $"{configuration["Oidc:Authority"]}/protocol/openid-connect/auth",
                            TokenUrl = $"{configuration["Oidc:Authority"]}/protocol/openid-connect/token",
                            Scopes = new Dictionary<string, string>
                            {
                                //{"openid", "openid"},
                            }
                        }
                    },
                });
            settings.OperationProcessors.Add(new ApiVersionProcessor());
            settings.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
            settings.OperationProcessors.Add(new AuthorizationOperationProcessor("bearer"));
            settings.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = this.GetType().Namespace;
                document.Info.Description = "Weather API";
                document.Info.TermsOfService = "http://www.weather.com";
                document.Info.Contact = new OpenApiContact
                {
                    Name = "John Doe",
                    Email = "info@weather.com",
                    Url = "http://www.weather.com"
                };
            };
        }

        private void ConfigureProblemDetails(ProblemDetailsOptions options)
        {
            options.IncludeExceptionDetails = (ctx, ex) => true;
            options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
            options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        }
    }
}
