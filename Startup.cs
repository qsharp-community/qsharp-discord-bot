// <copyright file="Startup.cs" company="https://qsharp.community/">
// Copyright (c) Chris Granade. Licensed under the MIT License.
// </copyright>

using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.IQSharp;

public class Startup
{
    public IConfiguration Configuration { get; init; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            // Add services needed for acting as a Discord application.
            .AddDiscord()
            // Add logging services.
            .AddLogging(
                builder => builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddAzureWebAppDiagnostics()
                    .AddConsole()
                )
            // Add services unique to this bot.
            .AddSingleton<CommandHandler>()
            .AddSingleton<Bot>();

        // Add services needed for working with the Q# compiler and runtime.
        services.AddIQSharp();
        
        // Add services needed for the bot health web app.
        services.AddRazorPages();
        services.AddServerSideBlazor();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }

}
