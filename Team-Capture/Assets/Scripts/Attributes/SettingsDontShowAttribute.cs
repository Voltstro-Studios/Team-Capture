using System;

namespace Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class SettingsDontShowAttribute : Attribute
	{
	}
}