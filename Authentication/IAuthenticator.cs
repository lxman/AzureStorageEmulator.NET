using System.Net;

namespace Authentication
{
    public interface IAuthenticator
    {
        bool Authenticate(HttpWebRequest request);
    }
}