using System.Runtime.CompilerServices;
using AvePoint.RA.CommonUtil;

namespace CommonUtility30.Tests;

internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        RALogger.ConfigFile = "TestLog4net.config";
    }
}