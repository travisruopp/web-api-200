using HelpDesk.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddOtel();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();


