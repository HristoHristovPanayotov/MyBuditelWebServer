

using BuditelWebServer.server.HTTP;
using BuditelWebServer.server.Responses;
using System.Text;
using System.Web;
using WebServer.Server;
using WebServer.Server.HTTP_Request;
using WebServer.Server.Responses;
using WebServer.Server.Views;

namespace WebServer.demo
{
    public class StartUp
    {


        private const string HtmlForm = @"<form action='/HTML' method='POST'>
    Name: <input type='text' name='Name' />
    Age: <input type='number' name='Age' />
    <input type='submit' value='Save' />
</form>";

        private const string DownloadForm = @"<form action='/Content' method='POST'>
    <input type='submit' value='Download Sites Content' />
</form>";

        private const string FileName = "context.txt";

        private const string LoginForm = @"<form action='/Login' method='POST'>
Username: <input type='text' name='Username'/>
Password: <input type='text' name='Password'/>
<input type='submit' value ='Log In' />
</form>";

        private const string Username = "user";

        private const string Password = "user123";
        public static async Task Main()
        {
            await DownloadSitesAsText(StartUp.FileName,
                new string[] { "https://judge.softuni.org/", "https://softuni.org/" });
            var server = new HttpServer(routes => routes
                .MapGet("/", new TextResponse("Hello from the server!"))
                .MapGet("/HTML", new HtmlResponse("<h1>HTML response</h1>"))
                .MapGet("/HTML", new TextResponse("", StartUp.AddFormDataAction))
                .MapGet("/Redirect", new RedirectResponse("https://softuni.org/"))
                .MapGet("/login", new HtmlResponse(Form.HTML))
                .MapPost("/login", new TextResponse("", AddFormDataAction))
                .MapGet("/Content", new HtmlResponse(StartUp.DownloadForm))
                .MapGet("/Cookies", new HtmlResponse(StartUp.DownloadForm, AddCookieActions))
                .MapPost("/Content", new TextFileResponse(StartUp.FileName))
                .MapGet("/Session", new TextResponse("",
                                              StartUp.DisplaySessionInfoAction))
                .MapGet("/Login", new HtmlResponse(StartUp.LoginForm))
                .MapPost("/Login", new TextResponse("", StartUp.LoginAction))
                .MapGet("/LogOut", new HtmlResponse("", StartUp.LogOutAction))
                .MapGet("/UserProfile", new HtmlResponse("",
                                                  StartUp.GetUserDataAction)));

            await server.Start();

        }






        private static void AddFormDataAction(
            Request request, Response response)
        {
            response.Body = "";

            foreach (var (key, value) in request.Form)
            {
                response.Body += $"{key} - {value}";
                response.Body += Environment.NewLine;
            }
        }

        private static async Task<string> DownloadWebSiteContent(string url)
        {

            var httpClient = new HttpClient();
            using (httpClient)
            {
                var response = await httpClient.GetAsync(url);

                var html = await response.Content.ReadAsStringAsync();

                return html.Substring(0, 2000);
            }
        }

        private static async Task DownloadSitesAsText(string fileName, string[] urls)
        {
            var downloads = new List<Task<string>>();

            foreach (var url in urls)
            {
                downloads.Add(DownloadWebSiteContent(url));
            }
            var responeses = await Task.WhenAll(downloads);
            var responseString = string.Join(
                Environment.NewLine + new String('-', 100), responeses);

            await File.WriteAllTextAsync(fileName, responseString);


        }

        private static void AddCookieActions(Request request, Response response)
        {
            var requestHasCookies = request.Cookies
                .Any(c => c.Name != Session.SessionCookieName);
            var bodyText = "";

            if (requestHasCookies)
            {
                var cookieText = new StringBuilder();
                cookieText.AppendLine("<h1>Cookies</h1>");
                cookieText.Append("<table border='1'><tr><th>Name</th><th>Value</th></tr>");

                foreach (var cookie in request.Cookies)
                {
                    cookieText.Append("<tr>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Name)}</td>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Value)}</td>");
                    cookieText.Append("</tr>");
                }

                cookieText.Append("</table>");

                bodyText = cookieText.ToString();
            }
            else
            {
                bodyText = "<h1>Cookies set!</h1>";
            }

            if (!requestHasCookies)
            {
                response.Cookies.Add("My-Cookie", "Cookie-Value");
                response.Cookies.Add("My-Second-Cookie", "Second-Cookie-Value");
            }

            response.Body = bodyText;
        }

        private static void DisplaySessionInfoAction
            (Request request, Response response)
        {
            var sessionExists = request.Session
                .ContainsKey(Session.SessionCurrentDateKey);

            var bodyText = "";

            if (sessionExists)
            {
                var currentDate = request.Session[Session.SessionCurrentDateKey];
                bodyText = $"Stored date: {currentDate}";
            }
            else
            {
                bodyText = "Current date stored!";
            }

            response.Body = "";
            response.Body += bodyText;

        }

        private static void LoginAction(Request request, Response response)
        {
            request.Session.Clear();

            var bodyText = "";

            var usernameMatches = request.Form["Username"] == StartUp.Username;
            var passwordMatches = request.Form["Password"] == StartUp.Password;

            if (usernameMatches && passwordMatches)
            {
                request.Session[Session.SessionCurrentDateKey] = "MyUserId";
                response.Cookies.Add(Session.SessionCookieName,
                    request.Session.Id);

                bodyText = "<h3>Login successful!</h3>";
            }
            else
            {
                bodyText = StartUp.LoginForm;
            }

            response.Body = "";
            response.Body += bodyText;
        }

        private static void LogOutAction(Request request, Response response)
        {
            request.Session.Clear();
            response.Body = "";
            response.Body += "<h3>Logged out successfully!</h3>";
        }

        private static void GetUserDataAction(Request request, Response response)
        {
            if (request.Session.ContainsKey(Session.SessionUserKey))
            {
                response.Body = "";
                response.Body += $"<h3>Currently logged-in user " +
                    $"is with username '{Username}'</h3>";
            }

            else
            {
                response.Body = "";
                response.Body += "<h3>You should first log in " +
                    "- <a href='/Login'>Login</a></h3>";
            }

        }
    }
}
