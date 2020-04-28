using System.IO.Compression;

namespace VoltBuilder
{
	public class DefaultZip : IZip
	{
		/// <inheritdoc/>
		public void CompressDir(string directoryToCompress, string outPath)
		{
			ZipFile.CreateFromDirectory(directoryToCompress, outPath);
		}
	}
}