using Kbs.IdoWeb.Api.Localization;
using Kbs.IdoWeb.Api.Middleware;
using Kbs.IdoWeb.Data.Public;
using Kbs.IdoWeb.Data.Authentication;
using Kbs.IdoWeb.Data.Determination;
using Kbs.IdoWeb.Data.Information;
using Kbs.IdoWeb.Data.Location;
using Kbs.IdoWeb.Data.Mapping;
using Kbs.IdoWeb.Data.Observation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using NLog.Web;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Kbs.IdoWeb.Api
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
			//Inject AppSettings
			services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));

			/*.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)*/
			;
			services.AddDbContext<AuthenticationContext>(options =>
			options.UseNpgsql(Configuration.GetConnectionString("IdentityConnection")));

			//Caching - to be tested
			services.AddMemoryCache();
			services.AddResponseCaching();

			services.AddDbContext<PublicContext> (options =>
				options.UseNpgsql(Configuration.GetConnectionString("DatabaseConnection")));

			services.AddDbContext<MappingContext>(options =>
				options.UseNpgsql(Configuration.GetConnectionString("DatabaseConnection")));

			services.AddDbContext<InformationContext>(options =>
				options.UseNpgsql(Configuration.GetConnectionString("DatabaseConnection")));

			services.AddDbContext<DeterminationContext>(options =>
				options.UseNpgsql(Configuration.GetConnectionString("DatabaseConnection")));

			services.AddDbContext<ObservationContext>(options =>
				options.UseNpgsql(Configuration.GetConnectionString("DatabaseConnection")));

			services.AddDbContext<LocationContext>(options =>
				options.UseNpgsql(Configuration.GetConnectionString("LocationConnection")));

			services.AddDefaultIdentity<ApplicationUser>(/*options =>
				options.SignIn.RequireConfirmedEmail = true*/)
				.AddErrorDescriber<LocalizedIdentityErrorDescriber>()
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<AuthenticationContext>();


			services.AddCors(o =>
			{
				o.AddPolicy("AnyOrigin", builder =>
				{
					builder
						.AllowAnyHeader()
						.AllowAnyOrigin()
						.AllowAnyMethod();
				});
				o.AddPolicy("OneOrigin", builder =>
				{
					builder
						.WithOrigins("http://idoweb.bodentierhochvier.de", "https://idoweb.bodentierhochvier.de", "http://web305.s193.goserver.host", "https://web305.s193.goserver.host", "http://bodentierhochvier.de", "https://bodentierhochvier.de", "http://185.15.246.2", "https://185.15.246.2")
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
			});

			services.Configure<IdentityOptions>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 8;
				options.Password.RequireUppercase = false;
				options.Password.RequireNonAlphanumeric = false;
			});

			//Jwt Authentication

			var key = Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:JWT_Secret"].ToString());

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = false;   //https not used
				options.SaveToken = false; //don't save token on server after successfull Authentication
				options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
				{
					ValidateIssuerSigningKey = true, //System will validate seccret key during token validation
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false,
					ClockSkew = TimeSpan.Zero
				};
			});

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// default HSTS value is 30 days, for changes see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseMiddleware<JwtInHeaderMiddleware>();
			app.UseAuthentication();
			app.UseHttpsRedirection();
			app.UseCors("OneOrigin");

            //app.UseCors("AnyOrigin");
            app.UseMvc();
		}
	}
}
