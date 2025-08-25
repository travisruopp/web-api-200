using System.Globalization;
using Demos.Api.Home;
using Marten;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddScoped<ClassDataService>(sp =>
//{
//    var options = new ClassDataServiceOptions { ConnectionString = "someDb", MaxStudents = 25, MinStucents = 8 };
//    return new ClassDataService(options);

//});
//builder.Services.AddClassDataService((o) =>
//{
//    o.ConnectionString = "some connection string";
//    o.MinStudents = 8;
//    o.MaxStudents = 16;
//});
var config = builder.Configuration.GetSection(ClassDataServiceOptions.OptionsName);
builder.Services.AddSingleton(config!); 

builder.Services.AddSingleton<ClassDataService>();


builder.Services.AddControllers();
//builder.Services.AddTransient<HitCounter>(); // create a new one of these EVERY TIME any controller or service needs the HitCounter.
//builder.Services.AddScoped<HitCounter>(); // Create a new one of these for each request.
builder.Services.AddSingleton<ICountHits>(sp =>
{
    // this method is still "lazy" it's not going to run this until the ICountHits is injected somewhere
    var id = Guid.Parse("b37bd889-591a-40b0-b97b-00f007866607");
    var ssf = sp.GetRequiredService<IServiceScopeFactory>();

    return new PersistentHitCounter(id, ssf);

}); // Just a single instance - shared across everything (ever request, every usage)
//builder.Services.AddSingleton<ICountHits, PersistentHitCounter>();
Dictionary<string, decimal> lookedupPrices = new()
{
    { "eggs", 2.99M }
};
await Task.Delay(300);
var service = new SomeSlowStartingService(lookedupPrices);

builder.Services.AddSingleton(service); // This registers an "instance" - so eager.

var connectionString = builder.Configuration.GetConnectionString("scratch") ?? throw new Exception("No Connection String");

builder.Services.AddMarten(c =>
{
    c.Connection(connectionString);

}).UseLightweightSessions(); // further configure this - this is a marten option, not all that interesting.
var app = builder.Build();

app.Use(async (context, next) =>
{
    // Everything that happens BEFORE you controller method is accessible here.
    Console.WriteLine("Got a Request");
   if( context.Request.Method == "GET")
    {
        Console.WriteLine("We have a get request");
        var hasAuth = context.Request.Headers.Authorization.FirstOrDefault();
        Console.WriteLine("Has an authorization header");

    }
    ;
     await next(context);
    // Everything AFTER your controller method is here.
    var mt = context.Response.Headers.ContentType.FirstOrDefault();
    Console.Write("Response content-type is", mt);
    Console.WriteLine("Sending a Response");
});
app.MapControllers(); // Look at the request for the method and the path, and direct to the right place.
app.Run();



public class SomeSlowStartingService
{
    private readonly Dictionary<string, decimal> _prices;
    public SomeSlowStartingService(Dictionary<string, decimal> prices)
    {
        _prices = prices;
    }
    public void DoIt()
    {
        Console.WriteLine("Doing some slow work.");
    }

}


public class  ClassDataService
{
    public ClassDataService(IOptions<ClassDataServiceOptions> options)
    {
        // use those options to set up this service.
        var cs = options.Value;
        var x = cs.ConnectionString;
    }
    // use that stuff in here.
}

public class ClassDataServiceOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxStudents { get;  set; }
    public int MinStudents { get;  set; }

    public static string OptionsName = "ClassDataService";
};

public static class ClassDataServiceExtensions
{
    public static IServiceCollection AddClassDataService(
        this IServiceCollection services,
        Action<ClassDataServiceOptions> configureOptions)
    {
        var options = new ClassDataServiceOptions();
        configureOptions(options);
        services.AddSingleton(options);
        services.AddScoped<ClassDataService>();
        return services;
        
    }
}