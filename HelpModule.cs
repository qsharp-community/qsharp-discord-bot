// <copyright file="HelpModule.cs" company="https://qsharp.community/">
// Copyright (c) Chris Granade. Licensed under the MIT License.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.IQSharp;
using Microsoft.Quantum.IQSharp.Common;
using Microsoft.Quantum.QsCompiler.SyntaxTokens;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.QsCompiler.Transformations.QsCodeOutput;
using Microsoft.Quantum.Simulation.Simulators;

/// <summary>
///     Module with Discord bot commands relating to simulating Q# code.
/// </summary>
public class HelpModule : ModuleBase<SocketCommandContext>
{
    private readonly IOperationResolver resolver;
    private readonly ILogger logger;
    private static readonly Emoji waiting = new Emoji("⌛");

    public HelpModule(ILogger<HelpModule> logger, IOperationResolver resolver)
    {
        this.resolver = resolver;
        this.logger = logger;
    }

    [Command("help", RunMode = RunMode.Async)]
    [Summary("Returns help about a Q# function or operation.")]
    public async Task Simulate(
        [Remainder]
        [Summary("The Q# function or operation name that you're interested in.")]
        string code
    )
    {
        logger.LogDebug("Got new %help message.");
        await this.Context.Message.AddReactionAsync(waiting);
        try
        {
            var op = resolver.Resolve(code);

            if (op == null)
            {
                await ReplyAsync($"No function or operation named {code} was found.");
                await this.Context.Message.AddReactionAsync(new Emoji("😢"));
                return;
            }

            var signature = op.Header.DeclarationSignature();

            string? summary = null;
            string? description = null;
            foreach (var attr in op.Header.Attributes)
            {
                if (attr.TypeId.ValueOr(null)?.Namespace == "Microsoft.Quantum.Documentation")
                {
                    if (attr.TryGetString("Summary", out var newSummary))
                    {
                        summary = newSummary;
                    }
                    else if (attr.TryGetString("Description", out var newDescription))
                    {
                        description = newDescription;
                    }
                }
            }

            var url = $"https://docs.microsoft.com/en-us/qsharp/api/qsharp/{op.FullName.ToLowerInvariant()}";
            await ReplyAsync(
                embed: new EmbedBuilder()
                    .WithTitle($"{op.FullName} API reference")
                    .AddField("Signature", signature)
                    .AddField("Summary", summary)
                    .AddField("Description", description)
                    .WithUrl(url)
                    .Build()
            );
        }
        finally
        {
            await Context.Message.RemoveReactionAsync(waiting, Context.Client.CurrentUser);
        }
    }

}
