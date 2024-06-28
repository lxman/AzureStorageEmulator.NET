namespace AzureStorageEmulator.NET.Assembly_Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BuildTimeAttribute(string buildTime) : Attribute
    {
        public DateTime BuildTime => DateTime.Parse(buildTime);
    }
}
