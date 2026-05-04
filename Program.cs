using System.Diagnostics;

namespace RenameFromList
{
    internal class Program
    {
        static ConsoleColor ColorDefaultForeground = ConsoleColor.Cyan;
        static ConsoleColor ColorHighlight = ConsoleColor.Cyan;
        static ConsoleColor ColorExample = ConsoleColor.DarkCyan;
        static ConsoleColor ColorWarning = ConsoleColor.Yellow;
        static ConsoleColor ColorError = ConsoleColor.Red;
        static ConsoleColor ColorSuccess = ConsoleColor.Green;
        static bool matchPartialNames = false;
        static bool keepSuffix = false;
        static string separator = ",";
        static string? splitSymbol = null;
        static string? csvFile = null;
        static string? inputDirectory = null;
        static string? outputDirectory = null;
        static bool deleteOldFile = true;
        static bool overWrite = true;

        static void Main(string[] args)
        {
            // ARGUMENTS:
            // RENAMEFROMLIST [csv filename] [csv separator] [filename split symbol]

            ColorDefaultForeground = Console.ForegroundColor;

            if (args.Length == 0)
            {
                ColoredWriteline("No CSV file specified, please add the filename as the first argument.\n" +
                    "Use /? for help.", ColorError);
                ShowHelpInfo();

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                Environment.Exit(1);
            }

            string firstArg = args[0].ToLower();

            if (firstArg == "/?"    || firstArg == "-?"    || firstArg == "--?" ||
                firstArg == "/help" || firstArg == "-help" || firstArg == "--help" ||
                firstArg == "/h"    || firstArg == "-h"    || firstArg == "--h")
            {
                ShowHelpInfo();
                Environment.Exit(0);
            }
            else
            {
                ProcessArguments(args);
            }

            if (csvFile == null && firstArg.StartsWith('/') == false)
            {
                ColoredWriteline($"Assuming first argument is file name {firstArg}", ColorHighlight);
                csvFile = firstArg;
            }

            if (csvFile == null)
            {
                ColoredWriteline($"No CSV file specified, use /f FILE.CSV", ColorError);
                Environment.Exit(1);
            }

            if (File.Exists(csvFile) == false)
            {
                ColoredWriteline($"File not found: {csvFile}", ColorError);
                Environment.Exit(1);
            }

            Console.WriteLine($"Loading file {csvFile}");

            string[] csvLines = File.ReadAllLines(csvFile);

            int count = 0;

            if (inputDirectory == null)
            {
                inputDirectory = Path.GetFullPath(".") + "";
            }
            Debug.WriteLine($"Current directory: {inputDirectory}");

            string[] filesInDirectory = Directory.GetFiles(inputDirectory);
            Console.WriteLine($"Found {filesInDirectory.Length} files in the directory");


            foreach (string line in csvLines)
            {
                count++;
                Console.WriteLine();
                ColoredWriteline($"{count:00} : {line}", ColorHighlight);
                string[] split = line.Split(separator);
                if (split.Length < 2)
                {
                    ColoredWriteline($"Error, expecting 2 values on line {count}, found {split.Length}, skipping line", ColorError);
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
                    string foundFileNoSuffix = Path.GetFileNameWithoutExtension(file);
                    string splitsuffix = "";
                    string extension = Path.GetExtension(file);

                    //Console.WriteLine($"     Check file: {file}");
                    //Console.WriteLine($"     without extension: {foundFileNoExtension}");
                    //Console.WriteLine($"     extension: {extension}");

                    if (splitSymbol != null)
                    {
                        int splitLocation = foundFileNoExtension.IndexOf(splitSymbol);
                        if (splitLocation >= 0)
                        {
                            if (keepSuffix)
                            {
                                splitsuffix = foundFileNoExtension[splitLocation..];
                            }
                            foundFileNoSuffix = foundFileNoExtension[..splitLocation];
                        }
                        
                        //Console.WriteLine($"Using split, new value: {foundFileNoExtension}, suffix {splitsuffix} ''");
                    }

                    bool matchFound = false;
                    if (matchPartialNames)
                    {
                        if (foundFileNoExtension.Contains(originalName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            matchFound = true;
                        }
                    }
                    else
                    {
                        if (foundFileNoSuffix.Equals(originalName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            matchFound = true;
                        }
                    }

                    if (matchFound)
                    {
                        matchesCount++;
                        //Console.WriteLine($"   Found match in list: {foundFileNoExtension}, extension {extension}");
                        string newFileNameWithExtension;
                        if (matchPartialNames)
                        {
                            newFileNameWithExtension = foundFileNoExtension.Replace(originalName, newName, StringComparison.InvariantCultureIgnoreCase) + extension;
                            Console.WriteLine($"{newFileNameWithExtension} = {foundFileNoExtension} .Replace {originalName} with {newName} + {extension}");
                        }
                        else
                        {
                            newFileNameWithExtension = newName + splitsuffix + extension;
                        }

                        //Console.WriteLine($"   New name: {newFileNameWithExtension}");

                        try
                        {
                            outputDirectory = inputDirectory; // TODO
                            string outfile = Path.Combine(outputDirectory, newFileNameWithExtension);

                            if (File.Exists(outfile))
                            {
                                if (overWrite)
                                {
                                    ColoredWriteline($"File already exists {newFileNameWithExtension}, overwriting it.", ColorWarning);
                                    RenameFile(file, newFileNameWithExtension, outfile);
                                }
                                else
                                {
                                    ColoredWriteline($"File already exists {newFileNameWithExtension}, skipping it.", ColorError);
                                }
                            }
                            else
                            {
                                RenameFile(file, newFileNameWithExtension, outfile);
                            }
                        }
                        catch (Exception ex)
                        {
                            ColoredWriteline($"Error: {ex.Message}", ColorError);
                        }
                    }
                }

                ColoredWriteline($"Found {matchesCount} matches for {originalName}", ColorDefaultForeground);
                
            }

        }

        private static void RenameFile(string file, string newFileNameWithExtension, string outfile)
        {
            File.Copy(file, outfile, true);
            if (deleteOldFile)
            {
                ColoredWriteline($"Renaming {Path.GetFileName(file)} to {newFileNameWithExtension}", ColorSuccess);
                File.Delete(file);
            }
            else
            {
                ColoredWriteline($"Copied {Path.GetFileName(file)} to {newFileNameWithExtension}", ColorSuccess);
            }
        }

        private static void ProcessArguments(string[] commandLineArgs)
        {
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                string currentArg = commandLineArgs[i];
                if (currentArg == string.Empty)
                {
                    ColoredWriteline("Zero length argument in position " + i + ", skipped.", ColorWarning);
                    continue;
                }
                string firstChar = currentArg.Substring(0, 1);
                if ((firstChar == "-") || (firstChar == "/"))
                {
                    string commandType = currentArg.Substring(1);
                    string? commandValue = null;
                    if (i + 1 < commandLineArgs.Length)
                    {
                        // if the next argument exists, and starts with a / or -, it's a new argument, not the value for the current argument
                        commandValue = commandLineArgs[i + 1];
                        if (commandValue == string.Empty)
                        {
                            ColoredWriteline("Zero length sub-argument to " + currentArg, ColorWarning);
                            commandValue = null;
                        }
                        else
                        {
                            string firstCharValue = commandValue.Substring(0, 1);
                            if ((firstCharValue == "-") || (firstCharValue == "/"))
                            {
                                commandValue = null;
                            }
                        }

                    }

                    commandType = commandType.ToLower();

                    switch (commandType)
                    {
                        case "f":
                        case "file":
                        case "csv":
                            csvFile = commandValue;
                            ColoredWriteline($"CSV file: {commandValue}", ColorDefaultForeground);
                            break;
                        case "d":
                        case "directory":
                        case "folder":
                            inputDirectory = commandValue;
                            if (inputDirectory != null)
                            {
                                ColoredWriteline($"Will rename files in directory: {Path.GetFullPath(inputDirectory)}", ColorDefaultForeground);
                            }
                            else
                            {
                                ColoredWriteline($"Warning: No directory specified after /d",ColorWarning);
                            }
                            break;
                        case "cs":
                        case "separator":
                            if (commandValue != null)
                            {
                                separator = commandValue;
                                ColoredWriteline($"Separator set to '{separator}'", ColorDefaultForeground);
                            }
                            else
                            {
                                ColoredWriteline("No value specified for separator", ColorWarning);
                            }
                            break;
                        case "sp":
                        case "split":
                            splitSymbol = commandValue;
                            ColoredWriteline($"Suffix split symbol set to '{splitSymbol}'", ColorDefaultForeground);
                            break;
                        case "strict":
                            matchPartialNames = false;
                            break;
                        case "loose":
                            matchPartialNames = true;
                            break;
                        case "keep":
                        case "keepsuffix":
                            keepSuffix = true;
                            break;
                        case "del":
                            deleteOldFile = true;
                            break;
                        case "nodel":
                            deleteOldFile = false;
                            break;
                        case "noow":
                        case "nooverwrite":
                            overWrite = false;
                            break;
                        default:
                            ColoredWriteline("Invalid argument passed: /" + commandType, ColorWarning);
                            Environment.Exit(0);
                            break;
                    }
                }
            }
        }

        private static void ShowHelpInfo()
        {
            Console.WriteLine("");
            Console.WriteLine("Renames files in a directory based on a CSV list. Entries should be listed without extensions.");
            Console.WriteLine("All matching files will be renamed / overwritten, of any file type.");
            Console.WriteLine("");

            ColoredWriteline("RENAMEFROMLIST [file.csv] [/f file.csv] [/d directory] [/cs ,] [/sp _] [/strict or /loose] [/keep] [/del or /nodel]", ColorSuccess);
            Console.WriteLine("");

            ColoredWriteline ("/f       /file       /csv", ColorHighlight);
            Console.WriteLine("   CSV file containing a list of name pairs to rename from and to (OLD,NEW)");
            Console.WriteLine("");

            ColoredWriteline ("/d       /directory  /folder", ColorHighlight);
            Console.WriteLine("   The folder containing the renamable files. Defaults to the working directory");
            Console.WriteLine("");

            ColoredWriteline ("/cs      /separator", ColorHighlight);
            Console.WriteLine("   Symbol used in the CSV to separate values, Default is comma");
            Console.WriteLine("");

            ColoredWriteline ("/sp      /split", ColorHighlight);
            Console.WriteLine("   Split Symbol, used in file names to indicate a suffix, will match files with the first part of the name,\n" +
                              "   ignoring the suffix" +
                              "   Ex: If the symbol is _, F01_01.pdf will be treated as F01.pdf");
            Console.WriteLine("");

            ColoredWriteline ("/loose", ColorHighlight);
            Console.WriteLine("   Will match and replace any file containing the OLD name with the NEW name, keeping any prefix and suffix\n" +
                              "   (split symbol is ignored)");
            Console.WriteLine("");

            ColoredWriteline("/strict", ColorHighlight);
            Console.WriteLine("   Will only match files using the exact name (split symbols can be used)");
            Console.WriteLine("");

            ColoredWriteline ("/keep    /keepsuffix", ColorHighlight);
            Console.WriteLine("   Keep any suffixes from the Split Symbol while using Strict match mode");
            Console.WriteLine("");

            ColoredWriteline("/del", ColorHighlight);
            Console.WriteLine("   Deletes the old files (Renames files)");
            Console.WriteLine("");

            ColoredWriteline("/nodel", ColorHighlight);
            Console.WriteLine("   Don't delete the old files (Copies files)");
            Console.WriteLine("");

            ColoredWriteline("/noow     /nooverwrite", ColorHighlight);
            Console.WriteLine("   Don't overwrite file if it already exists. By default files are overwritten.");
            Console.WriteLine("   If the file exists and can't be overwritten, the original file is not deleted.");
            Console.WriteLine("");

            Console.WriteLine("");
            ColoredWriteline("Example input csv", ColorHighlight);
            ColoredWriteline("OLDNAME,NEWNAME", ColorExample);
            ColoredWriteline("workfile-AA,delivery-AA", ColorExample);
            ColoredWriteline("workfile-BB,delivery-BB", ColorExample);
            Console.WriteLine("");
            ColoredWriteline("Rename result, using split symbol argument _:", ColorHighlight);
            ColoredWriteline("RENAMEFROMLIST.EXE example.csv , _", ColorExample);
            ColoredWriteline("workfile-AA.docx        > delivery-AA.docx", ColorExample);
            ColoredWriteline("workfile-AA.pdf         > delivery-AA.pdf", ColorExample);
            ColoredWriteline("workfile-BB_1234567.txt > delivery-BB.txt", ColorExample);
            Console.WriteLine("");
        }

        private static void ColoredWrite(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = ColorDefaultForeground;
        }

        private static void ColoredWriteline(string message, ConsoleColor color)
        {
            ColoredWrite(message + Environment.NewLine, color);
        }
    }
}
