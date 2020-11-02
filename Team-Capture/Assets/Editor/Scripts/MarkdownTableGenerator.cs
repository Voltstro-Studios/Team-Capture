using System;
using System.Text;

namespace Editor.Scripts
{
	/// <summary>
	/// Generates a markdown table depending on your options
	/// </summary>
	public class MarkdownTableGenerator
	{
		private readonly StringBuilder stringBuilder;
		private readonly int headersCount;

		/// <summary>
		/// Creates a new markdown table generator instance
		/// </summary>
		/// <param name="tile">Whats the title you want added on top</param>
		/// <param name="headers">What headers are there</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public MarkdownTableGenerator(string tile, params string[] headers)
		{
			if(headers.Length == 0)
				throw new ArgumentOutOfRangeException(nameof(headers));

			headersCount = headers.Length;

			stringBuilder = new StringBuilder();
			stringBuilder.Append($"## {tile}\n\n|");

			string headerUnderline = "\n|";

			foreach (string header in headers)
			{
				stringBuilder.Append($"{header}|");
				headerUnderline += $"{new string('-', header.Length)}|";
			}

			stringBuilder.Append($"{headerUnderline}\n");
		}

		/// <summary>
		/// Adds a new option to the table
		/// </summary>
		/// <param name="content"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void AddOption(params string[] content)
		{
			if(content.Length != headersCount)
				throw new ArgumentOutOfRangeException(nameof(content), "The content count doesn't meet how many header there are!");

			stringBuilder.Append("|");

			foreach (string option in content)
			{
				stringBuilder.Append($"{option}|");
			}

			stringBuilder.Append("\n");
		}

		/// <summary>
		/// Returns a string with the work-in-progress markdown table
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return stringBuilder.ToString();
		}
	}
}