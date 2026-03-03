using WebServer.Server.HTTP;
using WebServer.Server.HTTP_Request;

namespace WebServer.Server.Responses
{
    public class HtmlResponse : ContentResponse
    {
        public HtmlResponse(string html, 
            Action<Request, Response> preRenderAction = null)
            : base(html, ContentType.Html, preRenderAction)
        {
        }
    }
}
