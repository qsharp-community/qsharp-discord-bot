// <copyright file="Bot.cs" company="https://qsharp.community/">
// Copyright (c) Chris Granade. Licensed under the MIT License.
// </copyright>

using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

public class Bot
{
    private DiscordSocketClient client;

    private CommandHandler commands;

    public Bot(DiscordSocketClient client, CommandHandler commands)
    {
        this.commands = commands;
        this.client = client;
    }

    public async Task Start()
    {
        var token = System.Environment.GetEnvironmentVariable("TOKEN");
        if (token == null)
        {
            throw new Exception("No Discord API token provided; quitting.");
        }

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        await commands.InstallCommands();
    }
}
