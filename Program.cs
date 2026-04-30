using System.Diagnostics;

namespace RenameFromList
{
    internal class Program
    {
        static ConsoleColor defaultForegroundColor = ConsoleColor.Cyan;
        static void Main(string[] args)
        {
            // ARGUMENTS:
            // RENAMEFROMLIST [csv filename] [csv separator] [filename split symbol]

            defaultForegroundColor = Console.ForegroundColor;
            ConsoleColor highlight = ConsoleColor.Cyan;
            ConsoleColor example = ConsoleColor.DarkCyan;
            ConsoleColor warning = ConsoleColor.Yellow;
            ConsoleColor error = ConsoleColor.Red;
            ConsoleColor success = ConsoleColor.Green;

            string separator = ",";
            if (args.Length == 0)
            {
                Console.WriteLine("No CSV file specified, please add the filename as the first argument");
                Console.WriteLine("Use /? for more information");
                Environment.Exit(1);
            }

            if (args[0] == "/?")
            {
                Console.WriteLine("");
                Console.WriteLine("Renames files in a directory based on a CSV list. Entries should be listed without extensions.");
                Console.WriteLine("All matching files will be renamed / overwritten, of any file type.");
                Console.WriteLine("");
                ColoredWriteline ("RENAMEFROMLIST [csv filename] [csv separator character] [filename split symbol]", success);
                Console.WriteLine("");

                ColoredWrite     ("csv separator", highlight);
                ColoredWriteline ("            Defaults to comma. Optional if split symbol argument isn't used.", defaultForegroundColor);
                Console.WriteLine("");
                ColoredWrite     ("filename split symbol", highlight);
                ColoredWriteline ("    Optional. Files in the directory will be matched using", defaultForegroundColor);
                Console.WriteLine("                         the part of the name before the sybmol.");
                Console.WriteLine("                         Ex: If the symbol is _, F01_01.pdf will be treated as F01.pdf");

                Console.WriteLine("");
                ColoredWriteline ("Example input csv", highlight);
                ColoredWriteline ("OLDNAME,NEWNAME", example);
                ColoredWriteline ("workfile-AA,delivery-AA", example);
                ColoredWriteline ("workfile-BB,delivery-BB", example);
                Console.WriteLine("");
                ColoredWriteline ("Rename result, using split symbol argument _:", highlight);
                ColoredWriteline ("RENAMEFROMLIST.EXE example.csv , _", example);
                ColoredWriteline ("workfile-AA.docx        > delivery-AA.docx", example);
                ColoredWriteline ("workfile-AA.pdf         > delivery-AA.pdf", example);
                ColoredWriteline ("workfile-BB_1234567.txt > delivery-BB.txt", example);
                Console.WriteLine("");
                Environment.Exit(0);
            }

            string csvFile = args[0];

            if (args.Length >= 2)
            {
                separator = args[1];
                Console.WriteLine($"Separator symbol set to: {separator}");
            }

            string? splitSymbol = null;
            if (args.Length >= 3)
            {
                splitSymbol = args[2];
                Console.WriteLine($"Split symbol set to: {splitSymbol}");
            }


            Console.WriteLine($"Loading file {csvFile}");
            if (File.Exists(csvFile) == false)
            {
                ColoredWriteline($"File not found: {csvFile}", error);
                Environment.Exit(1);
            }

            string[] csvLines = File.ReadAllLines(csvFile);

            int count = 0;

            string directory = Path.GetFullPath(".")+"";
            Debug.WriteLine($"Current directory: {directory}");

            string[] filesInDirectory = Directory.GetFiles(directory);
            Console.WriteLine($"Found {filesInDirectory.Length} in the directory");

            foreach (string line in csvLines)
            {
                count++;
                Console.WriteLine();
                ColoredWriteline($"{count:00} : {line}", highlight);
                string[] split = line.Split(separator);
                if (split.Length < 2)
                {
                    ColoredWriteline($"Error, expecting 2 values on line {count}, found {split.Length}, skipping line", error);
                    continue;
                }
                string originalName = split[0];
                string newName = split[1];
                
                //string originalNoExtension = Path.GetFileNameWithoutExtension(originalName);
                //Console.WriteLine($"Filename without extension: {originalNoExtension}");

                int matchesCount = 0;
                foreach (string file in filesInDirectory)
                {
                    string foundFileNoExtension = Path.GetFileNameWithoutExtension(file);
                    
                    string extension = Path.GetExtension(file);

                    //Console.WriteLine($"     Check file: {file}");
                    Debug.WriteLine($"     without extension: {foundFileNoExtension}");
                    //Console.WriteLine($"     extension: {extension}");

                    if (splitSymbol != null)
                    {
                        foundFileNoExtension = foundFileNoExtension.Split(splitSymbol)[0];
                        Debug.WriteLine($"Using split, new value: {foundFileNoExtension}");
                    }

                    if (originalName == foundFileNoExtension)
                    {
                        matchesCount++;
                        Debug.WriteLine($"Found match in list: {foundFileNoExtension}, extension {extension}");
                        string newFileNameWithExtension = newName + extension;

                        try
                        {
                            File.Copy(file, newFileNameWithExtension, true);
                            File.Delete(file);
                            ColoredWriteline($"Renamed {foundFileNoExtension + extension} to {newFileNameWithExtension}", success);
                        }
                        catch (Exception ex)
                        {
                            ColoredWriteline($"Error: {ex.Message}", error);
                        }
                    }
                }

                ColoredWriteline($"Found {matchesCount} matches for {originalName}", defaultForegroundColor);
                
            }

        }

        private static void ColoredWrite(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = defaultForegroundColor;
        }

        private static void ColoredWriteline(string message, ConsoleColor color)
        {
            ColoredWrite(message + Environment.NewLine, color);
        }
    }
}
