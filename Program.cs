// <copyright file="Program.cs" company="https://qsharp.community/">
// Copyright (c) Chris Granade. Licensed under the MIT License.
// </copyright>

using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load environment variables from any .env files on the directory
        // path leading up from here. This allows us to set secrets as
        // environment variables on the server, but still locally test by
        // using .env files.
        DotNetEnv.Env.TraversePath().Load();

        // Start the various services making up the bot.
        var startup = new Startup();

        // Using the new startup object, actually start the bot.
        await startup.Start();

        // Await indefinitely before exiting the program.
        await Task.Delay(-1);
    }
}
