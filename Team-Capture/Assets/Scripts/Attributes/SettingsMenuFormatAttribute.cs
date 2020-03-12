using System;

namespace Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SettingsMenuFormatAttribute : Attribute
	{
		public SettingsMenuFormatAttribute(string menuFormat)
		{
			MenuNameFormat = menuFormat;
		}

		public string MenuNameFormat { get; set; }
	}
}
