using WebServer.Server.HTTP_Request;

namespace WebServer.Server.Responses
{
    public class NotFoundResponse : Response
    {
        public NotFoundResponse()
            : base(StatusCode.NotFound)
        {
        }
    }
}
