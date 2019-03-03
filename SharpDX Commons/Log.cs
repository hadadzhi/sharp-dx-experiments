using System;
using System.Threading;

namespace SharpDXCommons
{
	/// <summary>
	/// Simple logger
	/// </summary>
	public static class Log
	{
		private const string DATE_FORMAT = "dd-MM-yyyy HH:mm:ss.fff";

		public sealed class Level
		{
			public static Level ERROR = new Level(0, ConsoleColor.Red, "ERROR");
			public static Level INFO = new Level(1, ConsoleColor.Green, "INFO");
			public static Level DEBUG = new Level(2, ConsoleColor.Magenta, "DEBUG");

			public readonly int IntLevel;
			public readonly ConsoleColor Color;
			public readonly string Name;

			private Level(int intLevel, ConsoleColor color, string name)
			{
				IntLevel = intLevel;
				Color = color;
				Name = name;
			}
		}

		public static Level CurrentLevel = Level.DEBUG;
		public static bool LogEnabled = true;

		private static void WriteLogMessage(string message, Level level)
		{
			if (LogEnabled && level.IntLevel <= CurrentLevel.IntLevel)
			{
				ConsoleColor oldColor = Console.ForegroundColor;

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(DateTime.Now.ToString(DATE_FORMAT));

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write(" [Thread " + Thread.CurrentThread.ManagedThreadId + "]");

				Console.ForegroundColor = level.Color;
				Console.WriteLine(" [" + level.Name + "] " + message);

				Console.ForegroundColor = oldColor;
			}
		}

		public static void Error(string message)
		{
			WriteLogMessage(message, Level.ERROR);
		}

		public static void Info(string message)
		{
			WriteLogMessage(message, Level.INFO);
		}

		public static void Debug(string message)
		{
			WriteLogMessage(message, Level.DEBUG);
		}
	}
}
