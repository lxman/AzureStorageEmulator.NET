using System.Reflection;

namespace AzureStorageEmulator.NET.Assembly_Attributes.Readers
{
    public class BuildTime
    {
        public static DateTime GetBuildTime()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            BuildTimeAttribute? attribute = assembly.GetCustomAttribute<BuildTimeAttribute>();
            return attribute?.BuildTime ?? DateTime.MinValue;
        }
    }
}