namespace XmlTransformer
{
    public interface IXmlTransformer
    {
        string ToXml(object o, bool getRequest = false);

        object FromXml(string xml, Type type);
    }
}
