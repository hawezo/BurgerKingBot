using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using bkbot.Core;
using CefSharp;

namespace bkbot
{
    public class Program
    {

        private static int _width;
        private static int _height;
        private static string _title;

        private static BotSettings _settings;
        private static Bot _bot;

        static void Main(string[] args)
        {
            Program.Initialize();
            Task task = Task.Run(async () => {
                Program.DisplayGreeting();
                await Task.Delay(2500);
            });
            task.Wait();
            Program.Process();
            Program.WaitForExit();
        }

        private static void Initialize()
        {
            // settings
            _settings = BotSettings.Load();
            _settings.Save();

            // setting up console window
            _title = "Burger King Survey Bot (bkbot)";
            Console.Title = _title;
            Console.WindowWidth = 120;
            Console.WindowHeight = 30;

            _width = Console.BufferWidth;
            _height = Console.WindowHeight;

            // cef
            CefSettings settings = new CefSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                LogSeverity = LogSeverity.Warning
            };
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        private static void Process()
        {
            _bot = new Bot(_settings, true);
            List<string> codes = new List<string>();
            List<string> errors = new List<string>();
            Console.Title = _title + $" (1 of {_settings.amount.Clamp(1, 8)})";

            for (int i = 0; i < _settings.amount.Clamp(1, 8); i++)
            {
                string result = _bot.GetCode();
                Console.Title = _title + $" ({i} of {_settings.amount.Clamp(1, 8)})";
                if (!_bot.HasError)
                    codes.Add(result);
                else
                    errors.Add(result);
            }

            Console.Clear();
            if (!_bot.HasError)
                Program.DisplayCodes(string.Join("\n", codes.ToArray()));
            else
                Program.DisplayError(string.Join("\n", errors.ToArray()));
        }

        private static void WaitForExit()
        {
            ConsoleKey key = ConsoleKey.A;
            do
                key = Console.ReadKey().Key;
            while (key != ConsoleKey.Escape && key != ConsoleKey.Enter);
            Environment.Exit(0);
        }

        /// <summary>
        /// Displays codes in console in a fancy way
        /// </summary>
        private static void DisplayCodes(string codes)
        { 
            Console.CursorVisible = false;
            Program.Write(
                (_height / 2) - (codes.Height() + 4),
                "FINISHED FETCHING CODE.",
                ConsoleColor.DarkCyan);
            Program.Write(
                (_height / 2) - (codes.Height() + 3),
                "hawezo.xyz | github.com/hawezo",
                ConsoleColor.Gray);

            Program.Write(
                (_height / 2) - (codes.Height() + 1),
                Line(40, '_'),
                ConsoleColor.DarkGray);
            Program.Write(
                (_height / 2),
                codes,
                ConsoleColor.DarkYellow);
            Program.Write(
                (_height / 2) + codes.Height(),
                Line(40, '_'),
                ConsoleColor.DarkGray);
        }

        /// <summary>
        /// Displays an error in console in a fancy way
        /// </summary>
        private static void DisplayError(string error)
        {
            Console.CursorVisible = false;
            error += "\nYou may want to restart the bot to try again.";

            Program.Write(
                (_height / 2) - (error.Height() + 4),
                "AN ERROR OCCURED.",
                ConsoleColor.DarkRed);
            Program.Write(
                (_height / 2) - (error.Height() + 3),
                "hawezo.xyz | github.com/hawezo",
                ConsoleColor.Gray);

            Program.Write(
                (_height / 2) - (error.Height()),
                Line(40, '_'),
                ConsoleColor.DarkGray);
            Program.Write(
                (_height / 2),
                error,
                ConsoleColor.Red);
            Program.Write(
                (_height / 2) + error.Height(),
                Line(40, '_'),
                ConsoleColor.DarkGray);
        }

        /// <summary>
        /// Displays greeting with settings
        /// </summary>
        private static void DisplayGreeting()
        {
            Console.CursorVisible = false;
            Program.Write(
                ((_height / 2) - 3) - (3 + 4),
                "BURGER KING BOT",
                ConsoleColor.DarkCyan);
            Program.Write(
                ((_height / 2) - 3) - (3 + 3),
                "hawezo.xyz | github.com/hawezo",
                ConsoleColor.Gray);

            Program.Write(
                ((_height / 2) - 3) - (3),
                Line(40, '_'),
                ConsoleColor.DarkGray);

            // postal code
            Program.Write(
                (_width / 2) - (Line(40).Length / 2),
                ((_height / 2) - 3) - 1,
                "Postal Code: ",
                ConsoleColor.Gray);
            Program.Write(
                (_width / 2) + (Line(40).Length / 2) - _settings.postalCode.Length,
                ((_height / 2) - 3) - 1,
                _settings.postalCode,
                ConsoleColor.White);

            // ref
            Program.Write(
                (_width / 2) - (Line(40).Length / 2),
                (_height / 2) - 3,
                "Reference: ",
                ConsoleColor.Gray);
            Program.Write(
                (_width / 2) + (Line(40).Length / 2) - _settings.reference.Length,
                (_height / 2) - 3,
                _settings.reference,
                ConsoleColor.White);

            // amount
            Program.Write(
                (_width / 2) - (Line(40).Length / 2),
                ((_height / 2) - 3) + 1,
                "Codes: ",
                ConsoleColor.Gray);
            Program.Write(
                (_width / 2) + (Line(40).Length / 2) - _settings.amount.ToString().Length,
                ((_height / 2) - 3) + 1,
                _settings.amount.ToString(),
                ConsoleColor.White);

            Program.Write(
                ((_height / 2) - 3) + 2,
                Line(40, '_'),
                ConsoleColor.DarkGray);

            Console.SetCursorPosition(Console.CursorLeft, ((_height / 2) - 3) + 7);
        }

        #region Display Helpers

        private static string Line(int length, char character = '-')
        {
            string line = "";
            for (int i = 0; i < length; i++)
                line += character;
            return line;
        }

        private static void Write(int y, string text, ConsoleColor color)
        {
            string[] content = text.Split('\n', '\r').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            for (int i = 0; i < content.Length; i++)
                Program.Write(
                    (_width / 2) - (content[i].Length / 2),
                    (y - (content.Length / 2)) + i,
                    content[i],
                    color);
        }

        private static void Write(bool centered, int y, string text, ConsoleColor color)
        {
            string[] content = text.Split('\n', '\r').Where(x => !string.IsNullOrEmpty(x)).ToArray(); ;
            for (int i = 0; i < content.Length; i++)
                Program.Write(
                    !centered ? Console.CursorLeft : (_width / 2) - (content[i].Length / 2),
                    (y - (content.Length / 2)) + i,
                    content[i],
                    color);
        }

        private static void Write(int x, int y, string text, ConsoleColor color)
        {
            Console.SetCursorPosition(x, y);
            ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = c;

        }

        #endregion

    }


}
