using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Refit;
using Rookie.AMO.Identity.Business.Interfaces;
using Rookie.AMO.Identity.Business.Services;
using Rookie.AMO.Identity.DataAccessor;
using Rookie.AMO.Identity.DataAccessor.Data;
using Rookie.AMO.Identity.DataAccessor.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Rookie.AMO.Identity
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment CurrentEnvironment { get; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<User, IdentityRole>(options => {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IEmailSender, EmailSenderService>();


            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            services.AddMvc()
            .AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.SerializerSettings.DateFormatString = "dd'/'MM'/'yyyy";
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = c =>
                {


                    var errors = string.Join('\n', c.ModelState.Values.Where(v => v.Errors.Count > 0)
                        .SelectMany(v => v.Errors)
                        .Select(v => v.ErrorMessage));

                    return new BadRequestObjectResult(new
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        Message = errors
                    });
                };
            });

            services
                .AddRefitClient<IWebAPIProvider>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(Configuration.GetSection("AmoApiUrl").Value));

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            if (CurrentEnvironment.IsDevelopment())
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Bearer";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.ApiName = "api1";
                    options.Authority = "https://localhost:5001";
                });
            }
            else
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Bearer";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.ApiName = "api1";
                    options.Authority = "https://b3g1-amo-id4.azurewebsites.net";
                });
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ADMIN_ROLE_POLICY", policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
                options.AddPolicy("STAFF_ROLE_POLICY", policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Staff");
                });
                options.AddPolicy("DEFAULT_AUTHENTICATE_POLICY", policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.RequireAuthenticatedUser();
                });
                options.DefaultPolicy = options.GetPolicy("DEFAULT_AUTHENTICATE_POLICY");
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Place Info Service API",
                    Version = "v2",
                    Description = "Sample service for Learner",
                });
            });

            if (CurrentEnvironment.IsDevelopment())
            {
                services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(InitData.GetIdentityResources())
                .AddInMemoryClients(InitData.GetClients())
                .AddInMemoryApiScopes(InitData.ApiScopes)
                .AddInMemoryApiResources(InitData.ApiResources)
                .AddAspNetIdentity<User>();
            }
            else
            {
                var rsaCertificate = new X509Certificate2(
                Path.Combine(CurrentEnvironment.ContentRootPath, "idsrv3test.pfx"), "idsrv3test");

                services.AddIdentityServer()
                .AddSigningCredential(rsaCertificate)
                //.AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(InitData.GetIdentityResources())
                .AddInMemoryClients(InitData.GetClients())
                .AddInMemoryApiScopes(InitData.ApiScopes)
                .AddInMemoryApiResources(InitData.ApiResources)
                .AddAspNetIdentity<User>();
            }
            
            //seed data
            SeedIdentityData.EnsureSeedData(Configuration.GetConnectionString("DefaultConnection"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApplicationBuilder applicationBuilder)
        {
            if (CurrentEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseCors("AllowOrigins");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowOrigins");
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v2/swagger.json", "PlaceInfo Services"));

            if (CurrentEnvironment.IsDevelopment()) {
                //seed database
                var serviceScopeFactory = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
                using (var serviceScope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = serviceScope.ServiceProvider.GetService<AppIdentityDbContext>();
                    if (!dbContext.Database.CanConnect())
                    {
                        dbContext.Database.Migrate();
                    }
                }
            }

        }
    }
}
