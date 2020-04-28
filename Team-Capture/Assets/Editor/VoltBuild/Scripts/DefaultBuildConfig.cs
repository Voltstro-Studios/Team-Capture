using UnityEditor;

namespace VoltBuilder
{
	public class DefaultBuildConfig : IBuildConfig
	{
		/// <inheritdoc/>
		public BuildTarget BuildTarget { get; set; }

		/// <summary>
		/// Does a development build
		/// </summary>
		public bool DevBuild { get; set; }

		/// <summary>
		/// Copies PDB files to build
		/// </summary>
		public bool CopyPDBFiles { get; set; }

		/// <summary>
		/// Does a server (headless) build
		/// </summary>
		public bool ServerBuild { get; set; }

		/// <summary>
		/// Zip files after build
		/// </summary>
		public bool ZipFiles { get; set; }
	}
}