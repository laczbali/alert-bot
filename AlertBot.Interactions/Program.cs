using AlertBot.Interactions.Clients.AWS;
using AlertBot.Interactions.Clients.Discord;
using AlertBot.Interactions.Clients.Twilio;

var builder = WebApplication.CreateBuilder(args);

// Set up environment variable config provider
builder.Configuration.AddEnvironmentVariables("alertbot_");

// Add services to the container.
builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Configure custom services
builder.Services.AddSingleton<DiscordClient>();
builder.Services.AddSingleton<S3Client>();
builder.Services.AddSingleton<TwilioClient>();

// finish setup
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");

// perform startup tasks
var discordClient = app.Services.GetRequiredService<DiscordClient>();
await discordClient.RegisterGlobalCommands();

// run app
app.Run();
