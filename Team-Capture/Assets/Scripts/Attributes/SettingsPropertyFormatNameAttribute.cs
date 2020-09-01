using System;
using UnityEngine.Scripting;

namespace Attributes
{
	/// <summary>
	/// Tells the <see cref="Settings.DynamicSettingsUi"/> what the text should be, instead of the property name.
	/// <para>If this isn't applied, the <see cref="Settings.DynamicSettingsUi"/> will just use the field name.</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SettingsPropertyFormatNameAttribute : PreserveAttribute
	{
		public SettingsPropertyFormatNameAttribute(string menuFormat)
		{
			MenuNameFormat = menuFormat;
		}

		public string MenuNameFormat { get; set; }
	}
}
