using Microsoft.EntityFrameworkCore;
using StudyCycleCLI.Model;
using StudyCycleCLI.Util;
using static System.Double;
using static System.Int32;

namespace StudyCycleCLI
{
    internal class Program
    {
        static async Task Main(string[] args)
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

                if (entity == "cycle" && command == "create") await CreateCycleCommand(arguments);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Error(e.Message, e);
            }
        }

        static async Task CreateCycleCommand(string[] arguments)
        {
            const double factorTwo = 60.0;

            string? cycleTitle;
            double dailyStudyHours;
            var createdAt = DateTime.Now;

            do
            {
                Console.Write("Cycle Title: ");
                cycleTitle = Console.ReadLine();
            } while (cycleTitle != null && string.IsNullOrEmpty(cycleTitle.Trim()));
            
            do
            {
                Console.Write("How many hours are you willing to study daily? ");
                _ = TryParse(Console.ReadLine(), out dailyStudyHours);
            } while (dailyStudyHours == 0);

            List<StudyCycleSubject> subjects = [];
            ConsoleKeyInfo createAnotherSubject;

            do
            {
                Console.Clear();
                Console.WriteLine("[Adding subjects]");
                
                string? subjectTitle;
                int difficultyIndex;
                int amountOfContentIndex;
                double weight;

                do
                {
                    Console.Write("Subject Title: ");
                    subjectTitle = Console.ReadLine();
                } while (subjectTitle != null && string.IsNullOrEmpty(subjectTitle.Trim()));
                
                do
                {
                    Console.Write("Select a subject Difficulty: \n[0] Very Easy\n[1] Easy\n[2] Medium\n[3] Hard\n[4] Very Hard\n");
                    _ = TryParse(Console.ReadKey().KeyChar.ToString(), out difficultyIndex);
                } while (IsNegative(difficultyIndex) || difficultyIndex > 4);
                
                do
                {
                    Console.Write("\nHow much content does this subject have: \n[0] Very Little\n[1] Little\n[2] Medium\n[3] Much\n[4] Very Much\n");
                    _ = TryParse(Console.ReadKey().KeyChar.ToString(), out amountOfContentIndex);
                } while (IsNegative(amountOfContentIndex) || amountOfContentIndex > 4);
                
                do
                {
                    Console.Write("\nHow much WEIGHT this subject have: ");
                    _ = TryParse(Console.ReadLine(), out weight);
                } while (IsNegative(weight));

                var subject = new StudyCycleSubject()
                {
                    CompletedTimes = 0,
                    CreatedAt = DateTime.Now,
                    AmountOfContent = (AmountOfContent)amountOfContentIndex,
                    Difficulty = (Difficulty)difficultyIndex,
                    Weight = weight,
                    Title = subjectTitle!,
                    UpdatedAt = null,
                    StudiedHours = 0,
                    LastStudiedAt = null,
                    MaxStudyHours = 0
                };
                subjects.Add(subject);

                Console.Write("Add another subject? Yes = [Y]  No = [N]");
                createAnotherSubject = Console.ReadKey();
            } while (createAnotherSubject.Key == ConsoleKey.Y);

            var factorOne = subjects.Sum(subject => (int)Math.Round(((int)subject.Difficulty + 1 + (int)subject.AmountOfContent + 1) * subject.Weight));
            var factorThree = factorTwo / factorOne;

            foreach (var subject in subjects)
            {
                var subjectDifficulty = (int)subject.Difficulty + 1;
                var subjectAmountOfContent = (int)subject.AmountOfContent + 1;
                var subjectMaxStudyHours = (subjectDifficulty + subjectAmountOfContent) * subject.Weight * factorThree;
                
                subject.MaxStudyHours = (int)Math.Round(subjectMaxStudyHours);
            }

            var studyCycle = new StudyCycle()
            {
                Title = cycleTitle!,
                CompletedTimes = 0,
                CreatedAt = DateTime.Now,
                FactorOne = factorOne,
                LastStudiedAt = null,
                UpdatedAt = null,
                DailyStudyHours = dailyStudyHours,
                FactorThree = (int)factorThree,
                FactorTwo = (int)factorTwo,
                WeeklyStudyHours = dailyStudyHours * 7,
                Subjects = subjects
            };
            try
            {
                using var db = new AppDbContext();

                db.StudyCycles.Add(studyCycle);

                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
