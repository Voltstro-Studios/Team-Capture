using System;
using System.Text;

namespace Editor.Scripts
{
	public class MarkdownTableGenerator
	{
		private readonly StringBuilder stringBuilder;
		private readonly int headersCount;

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

		public override string ToString()
		{
			return stringBuilder.ToString();
		}
	}
}