using System;
using System.IO;

namespace CarCombiner {
    public static class Log {

        public static bool EnableLogging { get; set; } = true;

        private static bool OpenWrite = false;

        private static void WriteScaffolding(string text, ConsoleColor color, TextWriter stream) {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] >> ");
        }

        public static void WriteLine(object message, ConsoleColor color = ConsoleColor.Gray) {
            if (!EnableLogging) return;
            if (OpenWrite) Console.WriteLine();
            WriteScaffolding("INFO", ConsoleColor.Cyan, Console.Out);
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
            OpenWrite = false;
        }
        
        public static void Write(object message, ConsoleColor color = ConsoleColor.Gray) {
            if (!EnableLogging) return;
            if (OpenWrite) Console.WriteLine();
            WriteScaffolding("INFO", ConsoleColor.Cyan, Console.Out);
            Console.ForegroundColor = color;
            Console.Write(message);
            OpenWrite = true;
        }

        public static void CompleteWrite(object message) {
            if (!EnableLogging || !OpenWrite) return;
            Console.WriteLine(message);
            Console.ResetColor();
            OpenWrite = false;
        }

        public static void WriteWarning(object message, ConsoleColor color = ConsoleColor.Gray) {
            if (!EnableLogging) return;
            if (OpenWrite) Console.WriteLine();
            WriteScaffolding("WARNING", ConsoleColor.Yellow, Console.Out);
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
            OpenWrite = false;
        }
        
        public static void WriteError(object message, ConsoleColor color = ConsoleColor.Red) {
            if (OpenWrite) Console.WriteLine();
            WriteScaffolding("ERROR", ConsoleColor.Red, Console.Error);
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
            OpenWrite = false;
        }

        public static string RequestInput(string text) {
            if (OpenWrite) Console.WriteLine();
            Console.WriteLine("\n" + text);
            Console.Write("> ");
            string answer = Console.ReadLine();
            Console.WriteLine();
            OpenWrite = false;
            return answer;
        }

    }
}