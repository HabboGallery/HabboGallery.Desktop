using HabboGallery.Desktop.Utilities;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using HabboGallery.Desktop.Configuration;
using HabboGallery.Desktop;

var builder = Host.CreateApplicationBuilder(args);

// TODO: Configure services
// * InterceptionService
// * GalleryClient (HttpClient)
// etc.
builder.Services.Configure<GalleryOptions>(builder.Configuration);

builder.Services.AddWindowsFormsLifetime<MainFrm>();

await builder.Build().StartAsync();
