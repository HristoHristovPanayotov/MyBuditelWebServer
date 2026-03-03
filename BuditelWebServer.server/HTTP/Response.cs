
using BuditelWebServer.server.HTTP;
using System.Text;

namespace WebServer.Server.HTTP_Request
{
    public class Response
    {
        public StatusCode StatusCode { get; init; }
        public HeaderCollection Headers { get; } = new HeaderCollection();

        public CookieCollection Cookies { get; } = new CookieCollection();
        public string Body { get; set; }
        public Action<Request, Response> PreRenderAction { get; protected set; }
        public Response(StatusCode statusCode)
        {
            this.StatusCode = statusCode;

            this.Headers.Add(Header.Server, "My Web Server");
            this.Headers.Add(Header.Data, $"{DateTime.UtcNow:r}");
        }
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine($"HTTP/1.1 {(int)this.StatusCode} {this.StatusCode}");
            foreach (var header in this.Headers)
            {
                result.AppendLine(header.ToString());
            }
            foreach (var cookie in this.Cookies)
            {
                result.AppendLine($"{Header.SetCookie}: {cookie}");
            }
            result.AppendLine();
            if (!string.IsNullOrWhiteSpace(this.Body))
            {
                result.Append(this.Body);
            }
            return result.ToString();
        }
    }
}
