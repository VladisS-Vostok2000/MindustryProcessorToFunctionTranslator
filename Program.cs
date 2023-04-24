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
        }



        private static void ProcessAllFilesInDirectory(string directoryPath) {
            try {
                foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*" + ProcessedFileExtension)) {
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

            string[] finishedFile = ProcessFile(sr);

            StreamWriter sw;
            try {
                sw = new StreamWriter(Path.GetFileNameWithoutExtension(filePath) + OutFileExtension);
            }
            catch (IOException ioe) {
                Console.WriteLine($"Error with writing file. Check your rules.");
                Console.WriteLine($"Error was {ioe.Message}.");
                return;
            }

            WriteToFile(finishedFile, sw);
        }

        private static string[] ProcessFile(StreamReader sr) {
            string fileOneLine = sr.ReadToEnd();
            string[] file = fileOneLine.ToLines();
            List<Jump> jumps = ParseJumps(file);

            if (jumps.Count == 0) {
                return file;
            }

            List<Label> labels = GetLabels(GetPointers(jumps));
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
        private static List<Label> GetLabels(IEnumerable<int> labelsLocations) {
            List<Label> outList = new List<Label>(labelsLocations.Count());
            int i = 0;
            foreach (var labelLocation in labelsLocations) {
                outList.Add(new Label(labelLocation, "Label" + i.ToString()));
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


        private static void WriteToFile(string[] sourse, StreamWriter sw) {
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
