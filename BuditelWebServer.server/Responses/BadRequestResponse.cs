using WebServer.Server.HTTP_Request;

namespace WebServer.Server.Responses
{
    public class BadRequestResponse : HTTP_Request.Response
    {
        public BadRequestResponse()
            : base(StatusCode.BadRequest)
        {
        }
    }
}
