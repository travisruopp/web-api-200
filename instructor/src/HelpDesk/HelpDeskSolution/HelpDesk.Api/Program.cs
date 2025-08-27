

using System.Text.Json;
using HelpDesk.Api;
using System.Text.Json.Serialization;
using HelpDesk.Api.Employee.Issues;
using HelpDesk.Api.Users;
using Marten;

var builder = WebApplication.CreateBuilder(args);
builder.AddOtel();

builder.Services.AddControllers().AddJsonOptions(
    options =>
    {
        // options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper; // ImpactRadius -> IMPACT_RADIUS
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // No brainer.. Should be the default, imo. 
        //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;  // a little weirder, but I like it.
    });
builder.Services.AddOpenApi();
var connectionString = builder.Configuration.GetConnectionString("issues") ?? throw new Exception("No Connection String");
builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
}).UseLightweightSessions();
builder.Services.AddSingleton<IssueMetrics>();

builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
builder.Services.AddScoped<IProvideUserInfo, UserManager>();
builder.Services.AddHostedService<VipNotificationBackgroundWorker>();

//builder.Services.AddHttpClient(); // I am only going to call one remote api. Just use this one all the time. (I NEVER DO THIS, even when there is only one)
//builder.Services.AddHttpClient("helpdesk"); // Named clients. Jeff rating: "YUCK!"
builder.Services.AddHttpClient<CheckVipService>(config =>
{
    var apiUrl = builder.Configuration.GetConnectionString("help-desk") ?? throw new Exception("No Url for the Help Desk");
    config.BaseAddress = new Uri(apiUrl);
    //config.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Help-Desk-Api", ));

}); // Typed Clients


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers(); 
app.Run();

