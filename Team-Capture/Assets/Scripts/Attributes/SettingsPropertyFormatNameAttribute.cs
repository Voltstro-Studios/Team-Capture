using System;

namespace Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SettingsPropertyFormatNameAttribute : Attribute
	{
		public SettingsPropertyFormatNameAttribute(string menuFormat)
		{
			MenuNameFormat = menuFormat;
		}

		public string MenuNameFormat { get; set; }
	}
}
