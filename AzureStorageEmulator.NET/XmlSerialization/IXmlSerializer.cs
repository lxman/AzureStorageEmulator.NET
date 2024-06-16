namespace AzureStorageEmulator.NET.XmlSerialization
{
    public interface IXmlSerializer<T>
    {
        Task<string> Serialize(T o);

        T Deserialize(string xml);
    }
}