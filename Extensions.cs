// <copyright file="Extensions.cs" company="https://qsharp.community/">
// Copyright (c) Chris Granade. Licensed under the MIT License.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.SyntaxTokens;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.QsCompiler.Transformations.QsCodeOutput;
using QsExpression = Microsoft.Quantum.QsCompiler.SyntaxTokens.QsExpressionKind<
    Microsoft.Quantum.QsCompiler.SyntaxTree.TypedExpression,
    Microsoft.Quantum.QsCompiler.SyntaxTree.Identifier,
    Microsoft.Quantum.QsCompiler.SyntaxTree.ResolvedType
>;

public static class Extensions
{
    /// <summary>
    ///     Registers services used for interacting with Discord with a given
    ///     service collection.
    /// </summary>
    /// <returns>
    ///     A reference to <paramref name="services"/> after Discord services
    ///     have been added.
    /// </returns>
    public static IServiceCollection AddDiscord(this IServiceCollection services) =>
        services
            .AddSingleton<CommandService>()
            .AddSingleton<DiscordSocketClient>();

    /// <summary>
    ///     Emits a given Discord logging message as an ASP.NET Core Logging
    ///     message.
    /// </summary>
    public static Task Log<T>(this ILogger<T> logger, Discord.LogMessage message)
    {
        logger.Log(
            message.Severity switch
            {
                Discord.LogSeverity.Critical => LogLevel.Critical,
                Discord.LogSeverity.Debug => LogLevel.Debug,
                Discord.LogSeverity.Error => LogLevel.Error,
                Discord.LogSeverity.Info => LogLevel.Information,
                Discord.LogSeverity.Warning => LogLevel.Warning,
                _ => LogLevel.Trace
            },
            "{Message} ({Source})",
            message.Message, message.Source
        );
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Registers listeners for Discord logging events that forward
    ///     messages to ASP.NET Core logging services.
    /// </summary>
    /// <returns>
    ///     A reference to <paramref name="services" /> after logging event
    ///     listeners have been registered.
    /// </returns>
    public static IServiceProvider ConnectDiscordToLogging(this IServiceProvider services)
    {
        // Hook up ASP.NET Core logging to Discord services.
        services.GetRequiredService<CommandService>().Log +=
            services
            .GetRequiredService<ILogger<CommandService>>()
            .Log;
        services.GetRequiredService<DiscordSocketClient>().Log +=
            services
            .GetRequiredService<ILogger<DiscordSocketClient>>()
            .Log;
        return services;
    }

    public static bool TryGetString(this QsDeclarationAttribute? attribute, string name, out string? result, string @namespace = "Microsoft.Quantum.Documentation")
    {
        if (attribute?.TypeId.ValueOr(null)?.Name == name &&
            attribute.Argument.Expression is QsExpression.StringLiteral str)
        {
            result = str.Item1;
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public static string TypeParameterDeclaration(this ResolvedSignature signature) =>
        signature.TypeParameters switch
        {
            { IsEmpty: true } => "",
            var typeParams => $@"<{
                String.Join(", ", typeParams.Select(
                    t => t switch
                    {
                        QsLocalSymbol.ValidName name => name.Item,
                        _ => "__unknown__"
                    }
                ))
            }>"
        };

    public static string InputDeclaration(this CallableDeclarationHeader header) =>
        header.ArgumentTuple.InputDeclaration();

    public static string InputDeclaration(this QsTuple<LocalVariableDeclaration<QsLocalSymbol>> argumentTuple) =>
        argumentTuple switch
        {
            QsTuple<LocalVariableDeclaration<QsLocalSymbol>>.QsTuple { Item: var tuple } =>
                $@"({string.Join(", ", tuple.Select(
                    tupleItem => tupleItem.InputDeclaration()
                ))})",
            QsTuple<LocalVariableDeclaration<QsLocalSymbol>>.QsTupleItem { Item: var item } =>
                $@"{
                    item.VariableName switch
                    {
                        QsLocalSymbol.ValidName { Item: var name } => name,
                        _ => "__unknown__"
                    }
                } : {SyntaxTreeToQsharp.Default.ToCode(item.Type)}",
            _ => "__unknown__"
        };

    public static string DeclarationSignature(this CallableDeclarationHeader header)
    {
        return $"{header.QualifiedName.Name}{header.Signature.TypeParameterDeclaration()} {header.InputDeclaration()} : {SyntaxTreeToQsharp.Default.ToCode(header.Signature.ReturnType)}";
    }
}
