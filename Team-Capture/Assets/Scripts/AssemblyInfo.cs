using System.Reflection;
using System.Runtime.InteropServices;

// ReSharper disable once RedundantUsingDirective
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Team-Capture")]
[assembly: AssemblyDescription("The main game code")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Voltstro-Studios")]
[assembly: AssemblyProduct("Team-Capture")]
[assembly: AssemblyCopyright("Copyright © 2020 Voltstro-Studios")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("dd6d2cfe-a75e-42cc-bc37-766311b164e0")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.2")]
[assembly: AssemblyFileVersion("0.2")]

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
#endif