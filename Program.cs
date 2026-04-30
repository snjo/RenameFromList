namespace RenameFromList
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ARGUMENTS:
            // RENAMEFROMLIST [csv filename] [csv separator] [filename split symbol]

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
                Console.WriteLine("RENAMEFROMLIST [csv filename] [csv separator character] [filename split symbol]");
                Console.WriteLine("");
                
                Console.WriteLine("csv separator            Optional. Defaults to comma.");
                Console.WriteLine("");
                Console.WriteLine("filename split symbol    Optional. Files in the directory will be matched using");
                Console.WriteLine("                         the part of the name before the sybmol.");
                Console.WriteLine("                         Ex: If the symbol is _, F01_01.pdf will be treated as F01.pdf");

                Console.WriteLine("");
                Console.WriteLine("Example input csv");
                Console.WriteLine("OLDNAME,NEWNAME");
                Console.WriteLine("workfile-AA,delivery-AA");
                Console.WriteLine("workfile-BB,delivery-BB");
                Console.WriteLine("");
                Console.WriteLine("Rename result, using split symbol argument _:");
                Console.WriteLine("");
                Console.WriteLine("RENAMEFROMLIST.EXE example.csv , _");
                Console.WriteLine("workfile-AA.docx        > delivery-AA.docx");
                Console.WriteLine("workfile-AA.pdf         > delivery-AA.pdf");
                Console.WriteLine("workfile-BB_1234567.txt > delivery-BB.txt");
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
                Console.WriteLine($"File not found: {csvFile}");
                Environment.Exit(1);
            }

            string[] csvLines = File.ReadAllLines(csvFile);

            int count = 0;

            string directory = Path.GetFullPath(".")+"";
            Console.WriteLine($"Current directory: {directory}");

            string[] filesInDirectory = Directory.GetFiles(directory);
            Console.WriteLine($"Found {filesInDirectory.Length} in the directory");

            foreach (string line in csvLines)
            {
                count++;
                Console.WriteLine();
                Console.WriteLine($"{count:00} : {line}");
                string[] split = line.Split(separator);
                if (split.Length < 2)
                {
                    Console.WriteLine($"Error, expecting 2 values on line {count}, found {split.Length}, skipping line");
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
                    Console.WriteLine($"     without extension: {foundFileNoExtension}");
                    //Console.WriteLine($"     extension: {extension}");

                    if (splitSymbol != null)
                    {
                        foundFileNoExtension = foundFileNoExtension.Split(splitSymbol)[0];
                        Console.WriteLine($"Using split, new value: {foundFileNoExtension}");
                    }

                    if (originalName == foundFileNoExtension)
                    {
                        matchesCount++;
                        Console.WriteLine($"Found match in list: {foundFileNoExtension}, extension {extension}");
                        string newFileNameWithExtension = newName + extension;

                        try
                        {
                            Console.WriteLine($"Renaming {foundFileNoExtension + extension} to {newFileNameWithExtension}");
                            File.Copy(file, newFileNameWithExtension, true);
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                    }
                }

                Console.WriteLine($"Found {matchesCount} matches for {originalName}");
                
            }

        }
    }
}
