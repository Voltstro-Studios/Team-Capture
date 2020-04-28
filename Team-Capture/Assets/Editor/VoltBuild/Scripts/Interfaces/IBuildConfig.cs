using UnityEditor;

namespace VoltBuilder
{
	public interface IBuildConfig
	{
		/// <summary>
		/// The target to build to
		/// </summary>
		BuildTarget BuildTarget { get; set; }
	}
}