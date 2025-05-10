using StudyCycleCLI.Util;

namespace StudyCycleCLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello, World!");

                var entity = args[0];
                var command = args[1];
                var arguments = args.Skip(2).ToArray();
                var entityCommands = new Dictionary<string, string[]>()
                {
                    {
                        "cycle", 
                        [
                            "create",
                            "find",
                            "complete",
                            "view",
                            "edit", 
                            "addSubject", 
                            "study", 
                            "unstduy", 
                            "export", 
                            "delete"
                        ]
                    }
                };

                // --- [ Getting possible commands for each entity ] --- \\
                entityCommands.TryGetValue(entity, out var possibleCommands);
            
                // --- [ Validating Command & Entity ] --- \\
                if (possibleCommands == null) throw new Exception($"invalid entity '{entity}'.");

                if (!possibleCommands.Contains(command))
                {
                    throw new Exception($"invalid command '{command}' for entity '{entity}'.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Error(e.Message, e);
            }
        }
    }
}
