

/**
 * 
 * WebApplication.CreateBuilder, which creates a host for the 
 * website using defaults for a web host that is then built.
 * 
 */ 
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
Console.WriteLine("This executes after the web server has stopped!");

