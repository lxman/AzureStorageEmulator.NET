namespace AzureStorageEmulator.NET.XmlSerialization
{
    public interface IXmlSerializer<T>
    {
        string Serialize(T o);

        T Deserialize(string xml);
    }
}
