// <copyright file="Program.cs" company="https://qsharp.community/">
// Copyright (c) Chris Granade. Licensed under the MIT License.
// </copyright>

using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load environment variables from any .env files on the directory
        // path leading up from here. This allows us to set secrets as
        // environment variables on the server, but still locally test by
        // using .env files.
        DotNetEnv.Env.TraversePath().Load();

        // Build the web host, including all Discord bot services as well.
        var host = CreateHostBuilder(args).Build();
        // Start the Discord bot in the background.
        host.Services.ConnectDiscordToLogging();
        await host.Services.GetRequiredService<Bot>().Start();
        // Finally, run the web host.
        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
