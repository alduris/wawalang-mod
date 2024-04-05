using System;
using System.Collections.Generic;
using DevConsole.Commands;

namespace Wawalang
{
    // https://github.com/SlimeCubed/DevConsole/wiki/Writing-Commands
    internal static class ConsoleInterface
    {
        public static void Register()
        {
            var command = new CommandBuilder("wawalang");
            command.Run(Run);
            command.AutoComplete([
                // todo: replace with func
                ["help"],
                ["reset"],
                ["input"],
                ["run"],
                ["step", "<int>"]
            ]);
            command.Register();
        }

        private static void Run(string[] args)
        {
            //
        }
    }
}
