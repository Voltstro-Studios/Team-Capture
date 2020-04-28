namespace VoltBuilder
{
	public interface IZip
	{
		/// <summary>
		/// Compresses a directory into a zip
		/// </summary>
		/// <param name="directoryToCompress"></param>
		/// <param name="outPath"></param>
		void CompressDir(string directoryToCompress, string outPath);
	}
}