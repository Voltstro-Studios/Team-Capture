using System.Collections.Generic;

namespace VoltBuilder
{
	public class Config
	{
		/// <summary>
		/// The name of this project
		/// </summary>
		public string ProjectName { get; set; }

		/// <summary>
		/// Scenes included in this build
		/// </summary>
		public List<Scene> Scenes { get; set; }

		/// <summary>
		/// The directory (from the project path) to build to
		/// </summary>
		public string BuildDir { get; set; }

		public IBuildConfig BuildOptions;
	}
}