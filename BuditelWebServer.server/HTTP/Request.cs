//using BuditelWebServer.server.HTTP;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using BuditelWebServer.server.HTTP;
using System.Web;
using WebServer.Server.HTTP;
//using WebServer.Server.HTTP;
//using WebServer.Server.HTTP_Request;

namespace WebServer.Server.HTTP_Request
{
    public class Request
    {
        // Make Sessions static so it can be accessed from static methods
        private static Dictionary<string, Session> Sessions = new();

        public Method Method { get; private set; }
        public string Url { get; private set; }
        public HeaderCollection Headers { get; private set; }

        public CookieCollection Cookies { get; private set; }
        public string Body { get; private set; }

        public Session Session { get; private set; }

        
        public IReadOnlyDictionary<string, string> Form { get; private set; } = new Dictionary<string, string>();

        public static Request Parse(string request)
        {
            var lines = request.Split("\r\n");
            var startLine = lines.First().Split(" ");

            var method = ParseMethod(startLine[0]);

            var url = startLine[1];

            var headers = ParseHeaders(lines.Skip(1));

            var cookies = ParseCookies(headers);

            var session = GetSession(cookies);

            var bodyLines = lines.Skip(headers.Count + 2).ToArray();
            var body = string.Join("\r\n", bodyLines);

            var form = ParseForm(headers, body);

            return new Request
            {
                Method = method,
                Url = url,
                Headers = headers,
                Cookies = cookies,
                Body = body,
                Session = session,
                Form = form
            };
        }
        private static Method ParseMethod(string method)
        {
            try
            {
                return (Method)Enum.Parse(typeof(Method), method, true);
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Method '{method}' is not supportrd");
            }
        }
        private static HeaderCollection ParseHeaders(IEnumerable<string> headerLines)
        {
            var headers = new HeaderCollection();
            foreach (var headerLine in headerLines)
            {
                if (headerLine == string.Empty)
                {
                    break;
                }
                var headerParts = headerLine.Split(":", 2);
                if (headerParts.Length != 2)
                {
                    throw new InvalidOperationException("Request is not valid.");

                }

                var headerName = headerParts[0];
                var headerValue = headerParts[1].Trim();

                headers.Add(headerName, headerValue);
            }
            return headers;
        }

        private static Dictionary<string, string> ParseForm(HeaderCollection headers, string body)
        {
            var formCollection = new Dictionary<string, string>();

            if (headers.Contains(Header.ContentType)
                && headers[Header.ContentType] == ContentType.FormUrlEncodet)
            {
                var parsedForm = ParseFormData(body);

                foreach (var (name, value) in parsedForm)
                {
                    formCollection.Add(name, value);
                }
            }

            return formCollection;
        }
        private static Dictionary<string, string> ParseFormData(string bodyLines)
        {
            var formData = new Dictionary<string, string>();

            var pairs = bodyLines.Split('&');

            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');

                if (parts.Length == 2)
                {
                    var name = HttpUtility.UrlDecode(parts[0]);
                    var value = HttpUtility.UrlDecode(parts[1]);

                    formData[name] = value;
                }
            }

            return formData;
        }

        private static CookieCollection ParseCookies(HeaderCollection headers)
        {
            var cookieCollection = new CookieCollection();
            if (headers.Contains(Header.Cookie))
            {
                var cookieHeader = headers[Header.Cookie];
                var allCookies = cookieHeader.Split(';');
                foreach (var cookieText in allCookies)
                {
                    var cookieParts = cookieText.Split('=');

                    var cookieName = cookieParts[0].Trim();
                    var cookieValue = cookieParts[1].Trim();

                    cookieCollection.Add(cookieName, cookieValue);
                }
            }
            return cookieCollection;
        }

        private static Session GetSession(CookieCollection cookies)
        {
            var sessionId = cookies.Contains(Session.SessionCookieName)
                ? cookies[Session.SessionCookieName]
                : Guid.NewGuid().ToString();

            if (!Sessions.ContainsKey(sessionId)) 
            {
                Sessions[sessionId] = new Session(sessionId);
            }

            return Sessions[sessionId];
        }

    }
}
