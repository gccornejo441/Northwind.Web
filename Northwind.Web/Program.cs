

/**
 * 
 * WebApplication.CreateBuilder, which creates a host for the 
 * website using defaults for a web host that is then built.
 * 
 */ 
var builder = WebApplication.CreateBuilder(args);

/**
 * AddRazorPages() adds ASP.NET Core Razor Pages and its related services, 
 * such as model binding, authorization, anti-forgery, 
 * views, and tag helpers.
 */

builder.Services.AddRazorPages();

var app = builder.Build();

/**
 * The call to UseDefaultFiles must come before the call to UseStaticFiles
 */


app.UseDefaultFiles(); // index.xhtml, default.xhtml, and so on
app.UseStaticFiles();

app.MapRazorPages();
app.MapGet("/hello", () => "Hello World!");

app.Run();
Console.WriteLine("This executes after the web server has stopped!");

