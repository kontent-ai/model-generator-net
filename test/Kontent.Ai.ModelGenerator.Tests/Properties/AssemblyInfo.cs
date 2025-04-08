using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Kontent.Ai.ModelGenerator.Tests")]
[assembly: AssemblyTrademark("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9d6a9e5b-eb41-472a-90b2-df7e5eb43397")]

// TODO: Come up with something better...
// Hacky solution because StringWriter used by UserMessageLoggerTests
// gets polluted from ArgHelpersTests, wherein Console.Error.WriteLine() is used.
// Running the tests sequentially is slower but prevents pollution of Console.Error.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
