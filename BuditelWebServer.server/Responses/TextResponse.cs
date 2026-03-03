using WebServer.Server.HTTP;
using WebServer.Server.HTTP_Request;

namespace WebServer.Server.Responses
{
    public class TextResponse : ContentResponse
    {
        public TextResponse(string text, 
            Action<Request, Response> preRenderAction = null)
             : base(text, ContentType.PlainText, preRenderAction)
        {
        }
    }
}
