// <copyright file="CommandHandler.cs" company="https://qsharp.community/">
// Copyright (c) Chris Granade. Licensed under the MIT License.
// </copyright>

using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

public class CommandHandler
{
    private readonly DiscordSocketClient client;
    private readonly CommandService commands;
    private readonly IServiceProvider services;

    // Retrieve client and CommandService instance via ctor
    public CommandHandler(IServiceProvider services, DiscordSocketClient client, CommandService commands)
    {
        this.commands = commands;
        this.client = client;
        this.services = services;
    }

    public async Task InstallCommands()
    {
        // Handle incoming socket messages by checking if they're bot commands,
        // and then dispatching them accordingly.
        client.MessageReceived += HandleCommandAsync;

        // Search the current assembly for any command modules and
        // add them to the command service. Note that we provide our services
        // provider here, so that those services can be injected into command
        // modules as dependencies.
        await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                       services: services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is SocketUserMessage { Author: { IsBot: false }} message)
        {
            // Track where the prefix for the command is, whether that prefix
            // is a character or a direct mention of the bot.
            var argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('%', ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos)))
            {
                return;
            }

            // Create a WebSocket-based command context based on the message.
            var context = new SocketCommandContext(client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition
            // checks.
            await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: services);
        }
    }
}
