using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SongLyrics.Model.Settings;
using SongLyrics.Services;
using SongLyrics.Services.Interface;
using System.IO;
using System.Threading.Tasks;

internal class Program
{
    public static async Task Main(string[] args)
    {
        // create service collection
        var services = new ServiceCollection();
        ConfigureServices(services);

        // create service provider
        var serviceProvider = services.BuildServiceProvider();

        // entry to run app
        await serviceProvider.GetService<App>().Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // build config
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        // add services:        
        services.AddTransient<IWebApiService, WebApiService>();
        services.AddTransient<ISongLyricsService, SongLyricsService>();
        services.AddTransient<ILyricsOvhApiService, LyricsOvhApiService>();
        services.AddTransient<IMusicBrainzApiWrapperService, MusicBrainzApiWrapperService>();
        services.Configure<AppSettings>(configuration.GetSection("App"));
        services.AddMemoryCache();

        // add app
        services.AddTransient<App>();
    }
}