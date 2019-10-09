/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2019 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SpringCard.LibCs
{
    /**
	 * \brief Utility to create console line (CLI) tools with a consistent user experience.
	 */
    public abstract class CLIProgram
    {
        public static ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;
        public static ConsoleColor DefaultForegroundColor = Console.ForegroundColor;
        private ConsoleColor CurrentBackgroundColor = Console.BackgroundColor;
        private ConsoleColor CurrentForegroundColor = Console.ForegroundColor;
        public bool UseColors = true;

        public enum ConsoleColorScheme
        {
            Success,
            Failure,
            Info,
            Warning,
            Error,
            Title,
            Banner,
            Header,
            Normal
        }

        public void ConsoleColor()
        {
            ConsoleColor(DefaultBackgroundColor, DefaultForegroundColor);
        }

        public void ConsoleColor(ConsoleColorScheme colorScheme)
        {
            ConsoleColor backgroundColor = DefaultBackgroundColor;
            ConsoleColor foregroundColor = DefaultForegroundColor;

            switch (colorScheme)
            {
                case ConsoleColorScheme.Success:
                    backgroundColor = System.ConsoleColor.Black;
                    foregroundColor = System.ConsoleColor.Green;
                    break;
                case ConsoleColorScheme.Failure:
                    backgroundColor = System.ConsoleColor.Black;
                    foregroundColor = System.ConsoleColor.Red;
                    break;
                case ConsoleColorScheme.Info:
                    backgroundColor = System.ConsoleColor.Black;
                    foregroundColor = System.ConsoleColor.White;
                    break;

                case ConsoleColorScheme.Error:
                    backgroundColor = System.ConsoleColor.Red;
                    foregroundColor = System.ConsoleColor.Yellow;
                    break;
                case ConsoleColorScheme.Warning:
                    backgroundColor = System.ConsoleColor.DarkYellow;
                    foregroundColor = System.ConsoleColor.Black;
                    break;

                case ConsoleColorScheme.Title:
                    backgroundColor = System.ConsoleColor.Black;
                    foregroundColor = System.ConsoleColor.White;
                    break;
                case ConsoleColorScheme.Banner:
                    backgroundColor = System.ConsoleColor.Black;
                    foregroundColor = System.ConsoleColor.Cyan;
                    break;
                case ConsoleColorScheme.Header:
                    backgroundColor = System.ConsoleColor.DarkGray;
                    foregroundColor = System.ConsoleColor.Black;
                    break;

                case ConsoleColorScheme.Normal:
                default:
                    break;
            }

            ConsoleColor(backgroundColor, foregroundColor);
        }

        public void ConsoleColor(ConsoleColor foregroundColor)
        {
            ConsoleColor(DefaultBackgroundColor, foregroundColor);
        }

        public void ConsoleColor(ConsoleColor backgroundColor, ConsoleColor foregroundColor)
        {
            if (UseColors)
            {
                CurrentBackgroundColor = Console.BackgroundColor = backgroundColor;
                CurrentForegroundColor = Console.ForegroundColor = foregroundColor;
            }
        }

        public void ConsoleWriteLine(string line)
        {
            Console.Write(line);
            if (UseColors)
            {
                Console.BackgroundColor = DefaultBackgroundColor;
                Console.ForegroundColor = DefaultForegroundColor;
            }
            Console.WriteLine();
            if (UseColors)
            {
                Console.BackgroundColor = CurrentBackgroundColor;
                Console.ForegroundColor = CurrentForegroundColor;
            }
        }

        public void ConsoleTitle(string title)
        {
            ConsoleColor(System.ConsoleColor.White);
            ConsoleWriteLine(title);
            ConsoleColor();
        }

        public void ConsoleError(string text)
        {
            ConsoleColor(System.ConsoleColor.Red);
            ConsoleWriteLine(text);
            ConsoleColor();
        }
    }
}
