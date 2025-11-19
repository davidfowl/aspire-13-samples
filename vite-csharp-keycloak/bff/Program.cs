using Bff.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

// ⚠️ WARNING: This sample is for DEVELOPMENT ONLY
// Many security configurations here are not suitable for production.
// See README.md Security Notes section for details on production hardening.

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure forwarded headers to handle proxy (development only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// Add OpenAPI support
builder.Services.AddOpenApi();

// Configure Keycloak authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddKeycloakOpenIdConnect(
        "keycloak",
        realm: "demo",
        options =>
        {
            options.ClientId = "bff-client";
            options.ClientSecret = builder.Configuration["BFF_CLIENT_SECRET"] ?? throw new InvalidOperationException("BFF_CLIENT_SECRET is not configured.");
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            // ⚠️ WARNING: RequireHttpsMetadata = false allows HTTP for development but is vulnerable to MITM attacks
            // For production: Set to true and ensure Keycloak is only accessible via HTTPS
            options.RequireHttpsMetadata = false;
            options.MapInboundClaims = false; // Use standard OIDC claim names
            options.UsePkce = true;
            options.CallbackPath = "/api/auth/signin-oidc";
            options.SignedOutCallbackPath = "/api/auth/signout-callback-oidc";
        });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}

app.UseFileServer();

if (app.Environment.IsDevelopment())
{
    // Map OpenAPI and Scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

// Map authentication endpoints
app.MapAuthEndpoints();

// Map data endpoints (protected)
app.MapDataEndpoints();

app.Run();
