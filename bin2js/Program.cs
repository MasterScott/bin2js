﻿using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace bin2js {
    /// <summary>
    /// Some StringExtansion.
    /// </summary>
    public static class StringExtension {
        /// <summary>
        /// A string array which holds a lot of extansions, which we will use to compare against paths / filenames to determine the extansion of the actual work file.
        /// </summary>
        private static string[] extansions = new string[82] { ".bin", ".exe", ".elf", ".jpg", ".png", ".key", ".pdf", ".mp3", ".mp4", ".xml", ".html", ".htm", ".iso", ".psd", ".raw", ".zip", ".rar", ".php", ".css",
        ".wma", ".img", ".pst", ".reg", ".tiff", ".ini", ".log", ".txt", ".jpeg", ".mov", ".h24", ".apk", ".avi", ".db", ".sql", ".sdf", ".xslx", ".sys", ".csv", ".crypt", ".sav", ".mkv", ".thm", ".part", ".odt",
        ".oxps", ".mui", ".idx", ".dll", ".lic", ".lib", ".c", ".cpp", ".rb", ".vb", ".sln", ".ico", ".wud", ".pup", ".pdp", ".idx", ".s", ".js", ".vob", ".doc", ".m4a", ".a", ".asm", ".bmp", ".bat", ".asp", ".class",
        ".dmp", ".dump", ".gif", ".jar", ".java", ".mk", ".object", ".pl", ".rc", ".rdb", ".res" };
        
        /// <summary>
        /// Checks if a string do contain a specific string including StringCoparison option.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <param name="toCheck">The string that shall be looked for.</param>
        /// <param name="comparison">String Comparison options.</param>
        /// <returns>True if the string to look for was found., else false.</returns>
        public static bool Contains(this string source, string toCheck, StringComparison comparison) { return source.IndexOf(toCheck, comparison) >= 0; }

        /// <summary>
        /// Write a string to a file.
        /// </summary>
        /// <param name="destination">The file to write into.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="args">The arguments that shall be swapped with the place holders.</param>
        public static void WriteLine(this string destination, string message, [Optional] params object[] args) {
            try {
                using (StreamWriter sw = File.AppendText(destination)) {
                    using (TextWriter tw = sw) {
                        if (args != null) tw.WriteLine(message, args);
                        else tw.WriteLine(message);
                        tw.Close();
                    }
                }
            } catch (IOException io) { throw new Exception(io.ToString()); }
        }

        /// <summary>
        /// Trims the path out of a name. Name can be last folder in string or file.
        /// </summary>
        /// <param name="source">The path string to trim.</param>
        /// <returns>The name without any back slashes or path in it.</returns>
        public static string GetName(this string source) {
            string[] splitted = source.Split('\\');
            return splitted[splitted.Length - 1];
        }

        /// <summary>
        /// Trim the name out of a path. Name can be last folder in string or file.
        /// </summary>
        /// <param name="source">The path string to trim.</param>
        /// <returns>The path without the last folder or the file in the string.</returns>
        public static string GetPath(this string source) {
            string[] splitted = source.Split('\\');
            return source.Replace(splitted[splitted.Length - 1], "");
        }

        /// <summary>
        /// Tests if a file already exists and change the name if needed, then returns it. (Adds a number to the file on end before the extansion)
        /// </summary>
        /// <param name="source">The String to Test if the file exists.</param>
        /// <returns>The String that we can use to create a new file.</returns>
        public static string TestFileName(this string source) {
            try {
                string ext = ".new";                                                                                         // That string holds the extansion.
                for (int i = 0; i < extansions.Length; i++) {                                                                // Loop over the extansions array and compare against the file name.
                    if (source.Contains(extansions[i], StringComparison.InvariantCultureIgnoreCase)) {                       // If the extansions string from the array do match our file name...
                        ext = extansions[i];                                                                                 // ..we overload the matching extansion into the local extansion string holder.
                        break;                                                                                               // Break the loop.
                    }
                }

                if (File.Exists(source)) {                                                                                   // We check if the file exists.
                    for (int i = 0; i < 1000; i++) {                                                                         // We go trough an integer value up to 1000.
                        string test = Regex.Replace(source, ext, "", RegexOptions.IgnoreCase) + i.ToString() + ext;          // And test a new file name, where we added a number on the end.
                        if (!File.Exists(test)) {                                                                            // If the file do not exist.
                            if (test.Contains("0")) {                                                                        // We check first if this new name contains a 0.
                                File.Move(source, (Regex.Replace(source, ext, "", RegexOptions.IgnoreCase) + "0" + ext));    // If so we rename the original file and add the 0 here.
                                test = test.Replace("0", "1");                                                               // Then we rename the test file name and change the 0 to 1. This way it looks more nice within the folder.
                            }
                            source = test;                                                                                   // Set result to the new file name we can use.
                            break;                                                                                           // Break the loop.
                        }
                    }
                }
            } catch (IOException io) { throw new Exception(io.ToString()); }
            return source;                                                                                                   // Return the result.
        }
    }

    /// <summary>
    /// Some ByteExtansion.
    /// </summary>
    public static class ByteExtension {
        /// <summary>
        /// Write a byte array to file. Includes IO Exception Handling.
        /// </summary>
        /// <param name="source">The byte[] with data to write.</param>
        /// <param name="destination">The file to write into.</param>
        public static void Write(this byte[] source, string destination) {
            try {
                using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(destination, FileMode.Append, FileAccess.Write))) {
                    binaryWriter.Write(source);
                    binaryWriter.Close();
                }
            } catch (IOException io) { throw new Exception(io.ToString()); }
        }
    }

    class Program {
        /// <summary>
        /// The string to write into the .js.
        /// </summary>
        private const string u32 = "  u32[{0}] = 0x{1};";

        /// <summary>
        /// Check arguments.
        /// </summary>
        /// <param name="args">The arguments to check.</param>
        private static void CheckArgs(string[] args) {
            if (args.Length == 0 || args.Length > 1) ShowUsage();
            if (!args[0].Contains(".bin", StringComparison.InvariantCultureIgnoreCase)) {
                Console.WriteLine("Please feed me with a binary !");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Show Usage.
        /// </summary>
        private static void ShowUsage() {
            Console.WriteLine("Wrong Input !");
            Console.WriteLine("Usage: bin2js.exe <input.bin>");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadLine();
            Environment.Exit(0);
        }

        /// <summary>
        /// Read file into buffer.
        /// </summary>
        private static byte[] ReadIntoBuffer(string file, int length) {
            byte[] buffer = new byte[length];
            using (BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read))) {
                br.Read(buffer, 0, buffer.Length);
                br.Close();
            }
            return buffer;
        }

        /// <summary>
        /// The main entry.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args) {
            Console.WriteLine(" bin2js (c) by cfwprpht 2017\n");
            CheckArgs(args);

            FileInfo fi = new FileInfo(args[0]);                                                                                  // Get file informations.
            if (File.Exists(fi.FullName)) {                                                                                       // If file exists.
                ASCIIEncoding encode = new ASCIIEncoding();                                                                       // Encoder instance.
                try {
                    byte[] file = ReadIntoBuffer(args[0], (int)fi.Length);                                                        // Read file into buffer.
                    string newFile = (args[0].GetPath() + args[0].GetName().Replace(".bin", ".js")).TestFileName();               // Get the new file name.
                    File.Create(newFile).Close();                                                                                 // Create output file and check name availability.
                    newFile.WriteLine("// https:" + "//github.com/cfwprpht/bin2js\n");                                            // Write some repo info into the file.
                    byte[] trick = encode.GetBytes("function write" + newFile.GetName().Replace(".js", "") + "(write) {\n  setBase(write);\n"); // Encode the function string to bytes.
                    trick.Write(newFile);                                                                                         // Write the Function and name it like the input bin.

                    using (BinaryReader br = new BinaryReader(new MemoryStream(file))) {                                          // Initialize a binary reader and point him to the file buffer.
                        byte[] toStringify = new byte[4];                                                                         // Initialize a byte array to read the converting bytes into.
                        int index, readed;                                                                                        // Initialize some integer counters.
                        index = readed = 0;                                                                                       // Set them to 0.
                        while (readed != file.Length) {                                                                           // Loop over all bytes now and convert.
                            toStringify = br.ReadBytes(toStringify.Length);                                                       // Read 4 bytes.
                            newFile.WriteLine(u32, index.ToString(), BitConverter.ToString(toStringify).Replace("-", ""));        // Stringify the bytes, format and write it into the file.
                            index++;                                                                                              // Count indexer up.
                            readed += toStringify.Length;                                                                         // Count readed bytes up.
                        }
                    }
                    trick = encode.GetBytes("}\n");                                                                               // Encode the function closing clamb.
                    trick.Write(newFile);                                                                                         // Write the closing clamb for the function.
                } catch (Exception e) { throw new Exception(e.ToString()); }
            } else Console.WriteLine("Can not access the file !");                                                                // Error.
            Console.WriteLine("Done!\nThx for using my Tool ! :)\nPress any key to continue...");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
