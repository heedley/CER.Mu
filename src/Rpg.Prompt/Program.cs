﻿namespace CER.Rpg.Prompt
{
    using CER.Azure;
    using CER.Executable;
    using CER.Koan;
    using CER.Roslyn;
    using CER.Text;
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading;

    class Program : CER.Executable.Program
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static readonly string WarningTable = "CerRpgPrompt";
        private static Compiler RpgInterpreter = new Compiler();

        static void Main(string[] args)
        {
            Program.LogV("Entering CER.Rpg.Prompt.Program.Main");
            Program.LogI(
                string.Format("Running program {0}Prompt.exe{0}", Program.Separator) +
                string.Join(Program.Separator.ToString(), args));
            try
            {
                Program.ConnectAzureTableWarningListener();
                Program.InitializeInterpreter();
                Program.LogW("Completed Initialization.");

                Console.CancelKeyPress += delegate
                {
                    Program.CancellationTokenSource.Cancel();
                };
                Program.LogV("Entering main loop. Ctrl-C to exit.");
                string input;
                while (true)
                {
                    input = Console.ReadLine();
                    var tokenSequence = Program.RpgInterpreter.Scan(input).ToArray();
                    foreach (var token in tokenSequence)
                    {
                        Program.LogV(string.Join(Program.Separator.ToString(), token.Id, token.Value));
                    }
                }
            }
            catch (Exception e)
            {
                Program.LogE(e.DetailedMessage());
            }
        }

        public static void Query(Tokens command, params Token[] identifiers)
        {
            switch (command)
            {
                case Tokens.character:
                    Program.LogI(string.Format("{0}",
                        command.ToString()));
                    break;
                case Tokens.game:
                    Program.LogI(string.Format("{0}",
                        command.ToString()));
                    break;
                case Tokens.plot:
                    Program.LogI(string.Format("{0}",
                        command.ToString()));
                    break;
                default:
                    Program.LogW(string.Format(
                        "Command '{0}' has been passed to Program.Query",
                        command.ToString()));
                    break;
            }
        }

        private static void InitializeInterpreter()
        {
            Program.LogV("Initializing the Rpg interpreter.");
            Program.RpgInterpreter.Regex[Tokens.character] = "character ";
            Program.RpgInterpreter.Regex[Tokens.game] = "game ";
            Program.RpgInterpreter.Regex[Tokens.plot] = "plot ";
            Program.RpgInterpreter.Regex[Tokens.view] = "view ";
            Program.RpgInterpreter.Regex[Tokens.identifier] = @"\S+";

            Program.RpgInterpreter.Rules.Add(new Rule(Tokens.commands, Tokens.command, Tokens.commands));
            Program.RpgInterpreter.Rules.Add(new Rule(Tokens.commands, Tokens.command));
            Program.RpgInterpreter.Rules.Add(new Rule(Tokens.command, Tokens.character, Tokens.view, Tokens.identifiers));
            Program.RpgInterpreter.Rules.Add(new Rule(Tokens.command, Tokens.game, Tokens.view, Tokens.identifiers));
            Program.RpgInterpreter.Rules.Add(new Rule(Tokens.command, Tokens.plot, Tokens.view, Tokens.identifiers));
            Program.RpgInterpreter.Rules.Add(new Rule(Tokens.identifiers, Tokens.identifier, Tokens.identifiers));
            Program.RpgInterpreter.Rules.Add(new Rule(Tokens.identifiers, Tokens.identifier));
        }

        private static void ConnectAzureTableWarningListener()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["LogWarningsInAzureTable"]))
            {
                Program.LogV("Connecting AzureTableWarningListener to log warnings.");
                var azure_trace_listener = new AzureTableWarningListener
                {
                    Name = "AzureTableWarningListener",
                    CancellationToken = Program.CancellationTokenSource.Token,
                    Table_Name = Program.WarningTable,
                    Separator = Program.Separator,
                    Storage_Account = CloudStorageAccount.Parse(Koans.What_is_storage_when_lost_or_locked),
                };
                azure_trace_listener.Table.CreateIfNotExists();
                Program.External.Listeners.Add(azure_trace_listener);
            }
        }
    }

    public enum Tokens
    {
        character,
        game,
        plot,
        view,
        identifier,
        identifiers,
        command,
        commands,
    }
}
