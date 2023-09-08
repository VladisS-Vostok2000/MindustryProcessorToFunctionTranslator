using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MindustryProcessorToFunctionTranslator {
    public static class Program {
        public const string ProcessedFileExtension = ".min";
        public const string OutFileExtension = ".minfun";
        
        
        public const string LabelName = "Label";



        public static void Main(string[] args) {
            if (args.Count() == 0) {
                ProcessAllFilesInDirectory(".");
            }
            else
            if (args.Length == 1) {
                string path = args[0];
                if (File.Exists(path)) {
                    ProcessFileInPath(path);
                }
                else
                if (Directory.Exists(path)) {
                    ProcessAllFilesInDirectory(path);
                }
                else {
                    Console.WriteLine($"Invalid path: {path}");
                }
            }
            else {
                Console.WriteLine("Too many arguments.");
            }

            Console.WriteLine("Done.");
            Console.ReadKey(false);
        }



        private static void ProcessAllFilesInDirectory(string directoryPath) {
            try {
                foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*" + ProcessedFileExtension)) {
                    // For .net framework anomaly
                    // See the https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=net-7.0&redirectedfrom=MSDN#System_IO_Directory_EnumerateFiles_System_String_System_String_
                    if (!filePath.EndsWith(ProcessedFileExtension)) {
                        continue;
                    }

                    ProcessFileInPath(filePath);
                }
            }
            catch (IOException ioe) {
                // REFACTORING: определить в отдельный метод, который использует консоль для вывода ошибки.
                Console.WriteLine($"Can't process files in directory path \"{directoryPath}\". Check your rules.");
                Console.WriteLine($"Error was: {ioe.Message}.");
            }
        }

        /// <exception cref="ArgumentException"></exception>
        ///
        private static void ProcessFileInPath(string filePath) {
            if (Path.GetExtension(filePath) != ProcessedFileExtension) {
                // REFACTORING: определить в отдельный метод, использующий консоль для вывода ошибок.
                Console.WriteLine($"Filename extension is not \"{ProcessedFileExtension}\". Cant modify that file type.");
                return;
            }

            StreamReader sr;
            try {
                sr = new StreamReader(filePath);
            }
            catch (IOException ioe) {
                // REFACTORING: определить в отдельный метод, использующий консоль для вывода ошибок.
                Console.WriteLine($"Can't handle file on path \"{filePath}\". Check your rules.");
                Console.WriteLine($"Error was {ioe.Message}");
                return;
            }

            // First word of file.
            // TASK: create and use FirstWord() function.
            string processorName = Path.GetFileNameWithoutExtension(filePath).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
            
            string[] finishedFile = ProcessFile(sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries), processorName);

            string fileName = Path.ChangeExtension(filePath, OutFileExtension);
            WriteToFile(finishedFile, fileName);
        }

        private static string[] ProcessFile(string[] file, string processorName) {
            List<Jump> jumps = ParseJumps(file);

            if (jumps.Count == 0) {
                return file;
            }

            List<Label> labels = GetLabels(GetPointers(jumps), processorName);
            string[] processedFile = ExpandFileTwice(file);
            foreach (var label in labels) {
                processedFile[label.Location * 2] = label.LabelAsLabel;
            }

            processedFile = RemoveEmptyEntries(processedFile);
            SubstituteLabelsInJumps(processedFile, labels);
            return processedFile;
        }
        private static List<Jump> ParseJumps(string[] sourse) {
            var outList = new List<Jump>();
            for (int i = 0; i < sourse.Length; i++) {
                string line = sourse[i];
                bool isJump = Jump.TryParse(line, i, out Jump jump);
                if (!isJump) {
                    continue;
                }

                outList.Add(jump);
            }

            return outList;
        }
        private static HashSet<int> GetPointers(IEnumerable<Jump> jumps) {
            var outSet = new HashSet<int>();
            foreach (var jump in jumps) {
                outSet.Add(jump.To);
            }

            return outSet;
        }
        private static List<Label> GetLabels(IEnumerable<int> labelsLocations, string processorName) {
            List<Label> outList = new List<Label>(labelsLocations.Count());
            int i = 0;
            foreach (var labelLocation in labelsLocations) {
                string labelName = string.Concat(processorName, LabelName, i.ToString());
                outList.Add(new Label(labelLocation, labelName));
                i++;
            }

            return outList;
        }
        private static string[] ExpandFileTwice(string[] sourse) {
            string[] outString = new string[sourse.Length * 2];
            for (int i = 0; i < sourse.Length; i++) {
                outString[i * 2 + 1] = sourse[i];
            }

            return outString;
        }
        private static string[] RemoveEmptyEntries(string[] sourse) {
            int emptyEntriesCount = 0;
            foreach (var line in sourse) {
                if (line == null) {
                    emptyEntriesCount++;
                }
            }

            if (emptyEntriesCount == 0) {
                return sourse;
            }

            string[] outArray = new string[sourse.Length - emptyEntriesCount];
            int i = 0;
            foreach (var line in sourse) {
                if (line == null) {
                    continue;
                }

                outArray[i] = line;
                i++;
            }

            return outArray;
        }
        private static void SubstituteLabelsInJumps(string[] file, IEnumerable<Label> labels) {
            for (int i = 0; i < file.Length; i++) {
                string line = file[i];
                if (!Jump.TryParse(line, i, out Jump jump)) {
                    continue;
                }

                string[] arguments = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                arguments[Jump.PointerArgumentIndex] = labels.First(label => label.Location == jump.To).LabelAsPointer;
                file[i] = arguments.Join();
            }
        }


        private static void WriteToFile(string[] sourse, string filePath) {
            StreamWriter sw;
            try {
                sw = new StreamWriter(filePath);
            }
            catch (IOException ioe) {
                Console.WriteLine($"Error with writing file. Check your rules.");
                Console.WriteLine($"Error was {ioe.Message}.");
                return;
            }

            for (int i = 0; i < sourse.Length - 1; i++) {
                string line = sourse[i];
                sw.WriteLine(line.Trim());
            }
            sw.Write(sourse.Last().Trim());

            sw.Flush();
        }



        public static string[] ToLines(this string source) {
            return source.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static string Join(this IEnumerable<string> strings) {
            var sb = new StringBuilder();
            foreach (var @string in strings) {
                sb.Append(@string + " ");
            }

            return sb.ToString();
        }

    }
}
