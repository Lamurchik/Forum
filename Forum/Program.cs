using Forum.Model.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using NLog;
using NLog.Web;
using NLog.Extensions.Logging;
using OpenSearch.Net;
using Serilog.Events;
using Serilog.Sinks.OpenSearch;
using Serilog;
using System.Net;

using AutoRegisterTemplateVersion = Serilog.Sinks.OpenSearch.AutoRegisterTemplateVersion;
using CertificateValidations = OpenSearch.Net.CertificateValidations;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Forum.Model.GrachQL;



var logger1 = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();


logger1.Debug("init main");

var builder = WebApplication.CreateBuilder(args);


/*
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
*/
//builder.Host.UseNLog();



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => 
{
    options.AddSecurityDefinition("Bearer", 
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme() 
        { 
            In= ParameterLocation.Header,
            Description ="enter token",
            Type =SecuritySchemeType.Http,
            BearerFormat= "JWT",
            Scheme ="bearer"

        });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme() {
                Reference = new OpenApiReference()
                {
                    Type= ReferenceType.SecurityScheme,
                    Id= "Bearer"
                } 
            },
            new string[]{ }

        }
    });
    options.OperationFilter<FileUploadOperationFilter>();
    options.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
});
builder.Services.AddDbContext<ForumDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));///сделать перечисления 
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});





builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost"; // Ваш адрес Redis-сервера
    options.InstanceName = "local"; // Префикс для ключей Redis
});

//serialog

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

builder.Logging.ClearProviders();
Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
ServicePointManager.ServerCertificateValidationCallback = (o, certificate, chain, errors) => true;
ServicePointManager.ServerCertificateValidationCallback = CertificateValidations.AllowAll;

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("https://localhost:9200"))
    {
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1,
        MinimumLogEventLevel = LogEventLevel.Verbose,
        TypeName = "_doc",
        InlineFields = false,
        ModifyConnectionSettings = x =>
            x.BasicAuthentication("admin", "bars123@superMyPassword")
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                .ServerCertificateValidationCallback((o, certificate, chain, errors) => true),
        IndexFormat = "my-index-{0:yyyy.MM.dd}",
    })
    .CreateLogger();
    
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);




//  HotChocolate
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Forum.Model.GrachQL.Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions();






var app = builder.Build();


//var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
//loggerFactory.AddNLog();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ForumDBContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();   

app.UseHttpsRedirection();


app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.UseWebSockets();
app.MapGraphQL("/graphql");

app.Run();
app.Logger.LogInformation("starting up");
