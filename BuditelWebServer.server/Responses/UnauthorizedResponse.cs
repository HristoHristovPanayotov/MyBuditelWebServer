using WebServer.Server.HTTP_Request;

namespace WebServer.Server.Responses
{
    public class UnauthorizedResponse : Response
    {
        public UnauthorizedResponse()
            : base(StatusCode.Unauthorized)
        {
        }
    }
}
