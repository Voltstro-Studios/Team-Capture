using System.Collections.Generic;

namespace Core
{
	//CommandLineParser code borrowed from Packages.Rider.Editor.Util.CommandLineParser
	//https://docs.unity3d.com/Packages/com.unity.ide.rider@1.1/manual/index.html
	//
	//MIT License
	//Copyright (c) 2019 Unity Technologies

	/// <summary>
	/// Handles parsing the command line
	/// </summary>
	public static class CommandLineParser
	{
		/// <summary>
		/// All the options that were parsed
		/// </summary>
		public static Dictionary<string, string> Options = new Dictionary<string, string>();

		/// <summary>
		/// Parses arguments passed in
		/// </summary>
		/// <param name="args"></param>
		public static void ParseArgs(IReadOnlyList<string> args)
		{
			int i = 0;
			while (i < args.Count)
			{
				string arg = args[i];
				if (!arg.StartsWith("-"))
				{
					i++;
					continue;
				}

				string value = null;
				if (i + 1 < args.Count && !args[i + 1].StartsWith("-"))
				{
					value = args[i + 1];
					i++;
				}

				if (!(Options.ContainsKey(arg)))
				{
					Options.Add(arg, value);
				}
				i++;
			}
		}
	}
}