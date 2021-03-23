// <copyright file="Startup.cs" company="https://qsharp.community/">
// Copyright (c) Chris Granade. Licensed under the MIT License.
// </copyright>

using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.IQSharp;

public class Startup
{

    public IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection()
            .AddDiscord()
            .AddLogging(
                builder => builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddAzureWebAppDiagnostics()
                    .AddConsole()
                )
            .AddSingleton<CommandHandler>()
            .AddSingleton<Bot>();
        services.AddIQSharp();
        return services.BuildServiceProvider();
    }
    public async Task Start()
    {
        var services = BuildServiceProvider().ConnectDiscordToLogging();
        await services.GetRequiredService<Bot>().Start();
    }

}
