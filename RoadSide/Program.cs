using IndexedDB.Blazor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RoadSide;
using RoadSide.Middleware;
using RoadSide.Helpers;
using RoadSide.Services;
using RoadSide.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register root components for the Blazor WebAssembly app.
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- Authentication & user state ---
// Lightweight service that persists the JWT token to browser storage.
builder.Services.AddScoped<UserService>();

// Custom AuthenticationStateProvider implementation that reads the stored token
// and exposes the current user's identity to the Blazor auth system.
builder.Services.AddScoped<RoadSideAuthenticationStateProvider>();

// Register the custom provider as the AuthenticationStateProvider that Blazor will use.
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<RoadSideAuthenticationStateProvider>());

// Add authorization support for components (AuthorizeView, AuthorizeRouteView, etc.)
builder.Services.AddAuthorizationCore();

// --- App services ---
// In-memory cache for short-lived client-side caching.
builder.Services.AddMemoryCache();

// Default HttpClient for browser-relative requests (e.g., to static assets or relative endpoints).
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Named HttpClient "Api" configured to call the backend API.
// Adds a message handler (HttpTokenHandler) to attach the JWT to outgoing requests.
builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(builder.Configuration.GetSection("Api").GetValue<string>("BaseAddress")))
                //.ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                //{
                //  // Uncomment and customize if you need to relax server certificate validation in development.
                //  // ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                //
                //})
              .AddHttpMessageHandler<HttpTokenHandler>();

// Named HttpClient "Ping" for lightweight reachability checks to the API (no token handler).
builder.Services.AddHttpClient("Ping", client => client.BaseAddress = new Uri(builder.Configuration.GetSection("Api").GetValue<string>("BaseAddress")));

// IndexedDB factory used to persist offline data in the browser (IndexedDB.Blazor).
builder.Services.AddSingleton<IIndexedDbFactory, IndexedDbFactory>();

// Storage abstraction for reading/writing simple values (used by UserService).
builder.Services.AddScoped<IStorageService, StorageService>();

// Message handler that appends authorization header from persisted token.
builder.Services.AddScoped<HttpTokenHandler>();

// Application HTTP helpers for API calls and pinging.
builder.Services.AddScoped<IHttpService, HttpService>();
builder.Services.AddScoped<IHttpServicePing, HttpServicePing>();

await builder.Build().RunAsync();


