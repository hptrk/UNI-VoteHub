using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Voter.Blazor.WebAssembly;
using Voter.Blazor.WebAssembly.Infrastructure;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

// Add services
builder.Services.AddBlazorWebAssembly(builder.Configuration);

await builder.Build().RunAsync();
