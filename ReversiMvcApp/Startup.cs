using System;
using System.Threading.Tasks;
using Audit.Core;
using Audit.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ReversiMvcApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReversiMvcApp.Models;
using ReversiMvcApp.Services;

namespace ReversiMvcApp
{
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
            services.AddMvc(mvc =>
            {
                mvc.AddAuditFilter(config => config
                    .LogAllActions()
                    .WithEventType("{verb}.{controller}.{action}")
                    .IncludeHeaders(ctx => !ctx.ModelState.IsValid)
                    .IncludeRequestBody()
                    .IncludeModelState()
                    .IncludeResponseBody(ctx => ctx.HttpContext.Response.StatusCode == 200));
            });

            Audit.Core.Configuration.Setup()
                .UseSqlServer(config => config
                    .ConnectionString(Configuration.GetConnectionString("ReversiMvcAudit"))
                    .Schema("dbo")
                    .TableName("Event")
                    .IdColumnName("EventId")
                    .JsonColumnName("JsonData")
                    .LastUpdatedColumnName("LastUpdatedDate")
                    .CustomColumn("EventType", ev => ev.EventType)
                    .CustomColumn("User", ev => ev.Environment.UserName));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("ReversiMvcIdentity")));
            services.AddDbContext<ReversiDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ReversiMvcDb")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;

                    // Password requirements
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    // Unique email for every user
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.AddControllersWithViews();

            services.AddSingleton(_ => new ApiService(Configuration.GetValue<string>("ReversiApiUrl")));
            services.AddTransient<IEmailSender, EmailSender>();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromSeconds(1);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCookiePolicy(new CookiePolicyOptions
                {
                    Secure = CookieSecurePolicy.Always
                });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}