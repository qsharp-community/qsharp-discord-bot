// <copyright file="SimulateModule.cs" company="https://qsharp.community/">
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
using Microsoft.Quantum.Simulation.Simulators;

/// <summary>
///     Module with Discord bot commands relating to simulating Q# code.
/// </summary>
public class SimulateModule : ModuleBase<SocketCommandContext>
{
    private readonly ISnippets snippets;
    private readonly IWorkspace workspace;
    private readonly ILogger logger;
    private static readonly Emoji waiting = new Emoji("⌛");

    public SimulateModule(ISnippets snippets, IWorkspace workspace, ILogger<SimulateModule> logger)
    {
        this.snippets = snippets;
        this.workspace = workspace;
        this.logger = logger;
    }

    [Command("simulate", RunMode = RunMode.Async)]
    [Summary("Simulates a Q# snippet on the full-state simulator.")]
    public async Task Simulate(
        [Remainder]
        [Summary("The code to be simulated.")]
        string code
    )
    {
        logger.LogDebug("Got new %simulate message.");
        await this.Context.Message.AddReactionAsync(waiting);
        try
        {
            await workspace.Initialization;

            var snippet = snippets.Compile($@"operation RunDiscordCommand() : Unit {{
                {code}
            }}");

            foreach (var m in snippet.warnings)
            {
                await ReplyAsync($"[WARNING] {m}");
            }

            // Now actually run it.
            using var simulator = new QuantumSimulator();
            simulator.DisableLogToConsole();
            simulator.OnLog += async (msg) =>
            {
                var reply = await ReplyAsync($"➡ {msg}");
            };
            var operation = snippets.Operations.Single();

            var runMethod = operation.RoslynType.GetMethod("Run");
            if (runMethod == null)
            {
                throw new Exception("Run method not found on generated C# type.");
            }
            var result = runMethod.Invoke(null, new object[] { simulator });
            var response = await (dynamic)result!;


            await ReplyAsync(response.ToString());
        }
        catch (CompilationErrorsException cex)
        {
            await ReplyAsync($"Compilation error: {cex}");
        }
        finally
        {
            snippets.Items = Array.Empty<Snippet>();
            await Context.Message.RemoveReactionAsync(waiting, Context.Client.CurrentUser);
        }
    }

}
