using System.Data.Common;
using System.Security.AccessControl;
using System.Threading.Tasks;
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
                            "unstudy", 
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

                if (entity == "cycle" && command == "create"  ) { await CreateCycleCommandAsync  (arguments);  return; }
                if (entity == "cycle" && command == "find"    ) { FindStudyCycleByTitleSync      (arguments);  return; }
                if (entity == "cycle" && command == "complete") { await TryToCompleteCycleAsync  (arguments);  return; }
                if (entity == "cycle" && command == "view"    ) { ViewStudyCycleSync             (arguments);  return; }
                if (entity == "cycle" && command == "study"   ) { await StudyCycleSubjectAsync   (arguments);  return; }
                if (entity == "cycle" && command == "unstudy" ) { await UnstudyCycleSubjectAsync (arguments);  return; }
                if (entity == "cycle" && command == "delete"  ) { await DeleteStudyCycleAsync    (arguments);  return; }

                Console.WriteLine($"Command '{command}' of entity '{entity}' is not implemented yet.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Error(e.Message, e);
            }
        }

        // Commands
        static async Task DeleteStudyCycleAsync(string[] arguments)
        {
            if (arguments.Length == 0) throw new Exception("missing required argument: 'id'.");

            var successfullyParsedStudyCycleId = int.TryParse(arguments[0], out var id);
            if (!successfullyParsedStudyCycleId) throw new Exception($"'{arguments[0]}' is an invalid id.");

            using var db = new AppDbContext();

            var studyCycle = db.StudyCycles.Include(sc => sc.Subjects).Where(sc => sc.Id == id).FirstOrDefault();
            if (studyCycle == null) throw new Exception($"study cycle with id '{id}' was not found.");

            db.Remove<StudyCycle>(studyCycle);

            await db.SaveChangesAsync();

            Console.WriteLine($"'{studyCycle.Title}' was deleted!");
        }

        static async Task UnstudyCycleSubjectAsync(string[] arguments)
        {
            if (arguments.Length == 0) throw new Exception("missing required argument: 'study cycle id'.");
            if (arguments.Length < 3) throw new Exception("missing required argument: 'subject id'.");

            var successfullyParsedStudyCycleId = int.TryParse(arguments[0], out var studyCycleId);
            if (!successfullyParsedStudyCycleId) throw new Exception($"'{arguments[0]}' is an invalid id.");

            var successfullyParsedSubjectId = int.TryParse(arguments[2], out var subjectId);
            if (!successfullyParsedSubjectId) throw new Exception($"'{arguments[2]}' is an invalid id.");

            using var db = new AppDbContext();

            var studyCycle = db.StudyCycles.Include(sc => sc.Subjects).Where(sc => sc.Id == studyCycleId).FirstOrDefault();
            if (studyCycle == null) throw new Exception($"study cycle with id '{studyCycleId}' was not found.");

            var subject = studyCycle.Subjects.Where(sb => sb.Id == subjectId).FirstOrDefault();
            if (subject == null) throw new Exception($"subject with id '{subjectId}' was not found.");

            if (subject.StudiedHours == 0) throw new Exception($"'{subject.Title}' cannot be unstudied (studied hours is 0).");

            subject.StudiedHours -= 1;

            db.Update(studyCycle);

            await db.SaveChangesAsync();

            Console.WriteLine($"Removed 1h from '{subject.Title}'!");
        }

        static async Task StudyCycleSubjectAsync(string[] arguments)
        {
            if (arguments.Length == 0) throw new Exception("missing required argument: 'study cycle id'.");
            if (arguments.Length < 3) throw new Exception("missing required argument: 'subject id'.");

            var successfullyParsedStudyCycleId = int.TryParse(arguments[0], out var studyCycleId);
            if (!successfullyParsedStudyCycleId) throw new Exception($"'{arguments[0]}' is an invalid id.");

            var successfullyParsedSubjectId = int.TryParse(arguments[2], out var subjectId);
            if (!successfullyParsedSubjectId) throw new Exception($"'{arguments[2]}' is an invalid id.");

            using var db = new AppDbContext();

            var studyCycle = db.StudyCycles.Include(sc => sc.Subjects).Where(sc => sc.Id == studyCycleId).FirstOrDefault();
            if (studyCycle == null) throw new Exception($"study cycle with id '{studyCycleId}' was not found.");

            var subject = studyCycle.Subjects.Where(sb => sb.Id == subjectId).FirstOrDefault();
            if (subject == null) throw new Exception($"subject with id '{subjectId}' was not found.");

            if (subject.StudiedHours == subject.MaxStudyHours) throw new Exception($"'{subject.Title}' cannot be studied anymore (max reached).");

            subject.StudiedHours += 1;

            db.Update(studyCycle);

            await db.SaveChangesAsync();

            Console.WriteLine($"Added 1h to '{subject.Title}'!");
        }

        static void ViewStudyCycleSync(string[] arguments)
        {
            if (arguments.Length == 0) throw new Exception("missing required argument: 'id'.");

            var successfullyParsedStudyCycleId = int.TryParse(arguments[0], out var id);
            if (!successfullyParsedStudyCycleId) throw new Exception($"'{arguments[0]}' is an invalid id.");

            var lockedOrFreeSubjectsArg = arguments.Last();

            using var db = new AppDbContext();

            var studyCycle = db.StudyCycles.Include(sc => sc.Subjects).Where(sc => sc.Id == id).FirstOrDefault();
            if (studyCycle == null) throw new Exception($"study cycle with id '{id}' was not found.");

            List<StudyCycleSubject> subjects;

            switch (lockedOrFreeSubjectsArg)
            {
                case "--lockedSubjects" : subjects = studyCycle.Subjects.Where(sb => sb.StudiedHours == sb.MaxStudyHours).ToList(); break;
                case "--freeSubjects"   : subjects = studyCycle.Subjects.Where(sb => sb.StudiedHours <  sb.MaxStudyHours).ToList(); break;
                default                 : subjects = studyCycle.Subjects                                                          ; break;
            }

            var width = studyCycle.Title.Length + 40;
            var studyCycleTitle = $" [ {studyCycle.Title} ] ";

            // Info
            Console.Write("┌" + GetCenteredTextInline('─', width, studyCycleTitle) + "┐");
            Console.Write("\n│" + GetFieldWithPaddingRight("Id", studyCycle.Id.ToString(), 19).PadRight(width) + "│");
            Console.Write("\n│" + GetFieldWithPaddingRight("Total Subjects", studyCycle.Subjects.Count.ToString(), 19).PadRight(width) + "│");
            Console.Write("\n│" + GetFieldWithPaddingRight("Daily Study Hours", studyCycle.DailyStudyHours.ToString(), 19).PadRight(width) + "│");
            Console.Write("\n│" + GetFieldWithPaddingRight("Completed Times", studyCycle.CompletedTimes.ToString(), 19).PadRight(width) + "│");
            Console.Write("\n│" + GetFieldWithPaddingRight("Last Studied At", studyCycle.LastStudiedAt.ToString(), 19).PadRight(width) + "│");
            Console.Write("\n│" + GetFieldWithPaddingRight("Created At", studyCycle.CreatedAt.ToString(), 19).PadRight(width) + "│\n");
            Console.Write("└" + string.Concat(Enumerable.Repeat<string>("─", width)) + "┘");

            Console.WriteLine();

            // Subjects
            var subjectsBoxWidth = 150;
            var longestTitleLength = GetLongerSubjectTitle(subjects);

            Console.Write("╔" + GetCenteredTextInline('═', subjectsBoxWidth, " [ Subjects ] ") + "╗");
            foreach (var subject in subjects)
            {
                Console.Write("\n║" + GetFieldWithPaddingRight($"{subject.Id} - {subject.Title}", GetSubjectStudiedBoxes(subject.MaxStudyHours, subject.StudiedHours).ToString() + $"- {subject.StudiedHours}/{subject.MaxStudyHours}h", longestTitleLength).PadRight(subjectsBoxWidth) + "║");
            }
            Console.Write("\n╚" + string.Concat(Enumerable.Repeat<string>("═", subjectsBoxWidth)) + "╝");
        }

        static async Task TryToCompleteCycleAsync(string[] arguments)
        {
            if (arguments.Length == 0) throw new Exception("missing required argument 'ID'.");

            var successedConvertingIdToInt = int.TryParse(arguments[0], out int studyCycleId);
            if (!successedConvertingIdToInt) throw new Exception($"'{arguments[0]}' is not a valid id.");

            using var db = new AppDbContext();

            var studyCylce = db.StudyCycles.Include(sc => sc.Subjects).Where(sc => sc.Id == studyCycleId).FirstOrDefault();

            if (studyCylce == null) throw new Exception($"study cycle with id '{studyCycleId}' was not found.");

            foreach (var subject in studyCylce.Subjects)
            {
                if (subject.StudiedHours < subject.MaxStudyHours) throw new Exception("study cycle cannot be completed; finish all subjects first.");
            }

            // Complete it
            foreach (var subject in studyCylce.Subjects)
            {
                subject.StudiedHours = 0;
                subject.CompletedTimes += 1;
            }

            studyCylce.CompletedTimes += 1;
            studyCylce.LastStudiedAt = DateTime.Now;

            db.Update<StudyCycle>(studyCylce);

            await db.SaveChangesAsync();

            Console.WriteLine("Study cycle completed & Reseted!");
        }

        static void FindStudyCycleByTitleSync(string[] arguments)
        {
            if (arguments.Length == 0) throw new Exception("missing title.");

            var title = arguments[0];

            using var db = new AppDbContext();

            var studyCycle = db.StudyCycles.Include(sc => sc.Subjects).ToArray().Where(sc => sc.Title.ToUpper() == title.ToUpper()).FirstOrDefault();

            if (studyCycle == null) throw new Exception($"study cycle '{title}' was not found.");

            var width = studyCycle.Title.Length + 40;
            var studyCycleTitle = $" [ {studyCycle.Title} ] ";

            // Info
            Console.Write(  "┌" + GetCenteredTextInline    ('─', width, studyCycleTitle)                                                    + "┐"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Id", studyCycle.Id.ToString(), 19).PadRight(width)                             + "│"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Total Subjects", studyCycle.Subjects.Count.ToString(), 19).PadRight(width)     + "│"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Daily Study Hours", studyCycle.DailyStudyHours.ToString(), 19).PadRight(width) + "│"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Completed Times", studyCycle.CompletedTimes.ToString(), 19).PadRight(width)    + "│"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Last Studied At", studyCycle.LastStudiedAt.ToString(), 19).PadRight(width)     + "│"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Created At", studyCycle.CreatedAt.ToString(), 19).PadRight(width)              + "│\n");
            Console.Write(  "└" + string.Concat            (Enumerable.Repeat<string>("─", width))                                          + "┘"  );

            Console.WriteLine();

            // Config
            Console.Write(  "┌" + GetCenteredTextInline    ('─', width, " [ Config ] ")                                            + "┐"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Factor One", studyCycle.FactorOne.ToString(), 14).PadRight(width)     + "│"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Factor Two", studyCycle.FactorTwo.ToString(), 14).PadRight(width)     + "│"  );
            Console.Write("\n│" + GetFieldWithPaddingRight ("Factor Three", studyCycle.FactorThree.ToString(), 14).PadRight(width) + "│\n");
            Console.Write(  "└" + string.Concat            (Enumerable.Repeat<string>("─", width))                                 + "┘"  );
        }

        static async Task CreateCycleCommandAsync(string[] arguments)
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

        // Utils
        static int GetLongerSubjectTitle(List<StudyCycleSubject> subjects)
        {
            var bigger = 0;

            foreach (var subject in subjects)
            {
                var titleStudiedHoursAndMaxStudyHours = $" {subject.StudiedHours}/{subject.MaxStudyHours}h - {subjects.First().Title}:";
                if (titleStudiedHoursAndMaxStudyHours.Length > bigger) bigger = titleStudiedHoursAndMaxStudyHours.Length;
            }

            return bigger;
        }

        static string GetSubjectStudiedBoxes(int maxStudyHours, int studiedHours)
        {
            List<string> subjectStudiedBoxes = new List<string>();

            for (int i = 0; i < studiedHours; i++)
            {
                subjectStudiedBoxes.Add("■ ");
            }

            for (int i = 0; i < maxStudyHours - studiedHours; i++)
            {
                subjectStudiedBoxes.Add("□ ");
            }

            return string.Join("", subjectStudiedBoxes);
        }

        static string GetCenteredTextInline(char line, int baseWidth, string text)
        {
            return string.Concat(Enumerable.Repeat<char>(line, (baseWidth - text.Length) / 2)) + text + string.Concat(Enumerable.Repeat<char>(line, (baseWidth - text.Length) / 2));
        }
        
        static string GetFieldWithPaddingRight(string field, string content, int paddingAmount)
        {
            return $" {field}".PadRight(paddingAmount) + ":" + $" {content}";
        }
    }
}
