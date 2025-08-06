
using IndexedDB.Blazor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RoadSide;
using RoadSide.Middleware;
using RoadSide.Helpers;
using RoadSide.Services;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
       .CreateLogger();

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoadSideAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<RoadSideAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddMemoryCache();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(builder.Configuration.GetSection("Api").GetValue<string>("BaseAddress")))
                //.ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                //{
                //  //  ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }

                //})
              .AddHttpMessageHandler<HttpTokenHandler>();

builder.Services.AddHttpClient("Ping", client => client.BaseAddress = new Uri(builder.Configuration.GetSection("Api").GetValue<string>("BaseAddress")));

builder.Services.AddSingleton<IIndexedDbFactory, IndexedDbFactory>();

builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<HttpTokenHandler>();

builder.Services.AddScoped<IHttpService, HttpService>();

builder.Services.AddScoped<IHttpServicePing, HttpServicePing>();

await builder.Build().RunAsync();


