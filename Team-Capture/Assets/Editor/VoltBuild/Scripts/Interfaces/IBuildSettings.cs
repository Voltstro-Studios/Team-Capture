namespace VoltBuilder
{
	public interface IBuildSettings
	{
		/// <summary>
		/// Draws build settings GUI
		/// </summary>
		/// <param name="buildTool"></param>
		void DrawBuildSettings(BuildTool buildTool);
	}
}