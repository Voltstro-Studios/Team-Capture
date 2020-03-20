using System;

namespace Attributes
{
	/// <summary>
	/// Doesn't show this property or field in the settings menu
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class SettingsDontShowAttribute : Attribute
	{
	}
}