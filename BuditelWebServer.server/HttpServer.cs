using BuditelWebServer.server.HTTP;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebServer.Server.Contracts;
using WebServer.Server.HTTP;
using WebServer.Server.HTTP_Request;


namespace WebServer.Server
{
    public class HttpServer
    {
        private readonly IPAddress ipAddress;
        private readonly int port;
        private readonly TcpListener serverListener;
        private readonly RoutingTable routingTable;

        public HttpServer(string ipAddress, int port, Action<IRoutingTable> routingTableConfiguration)
        {

            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.serverListener = new TcpListener(this.ipAddress, port);

            routingTableConfiguration(this.routingTable = new RoutingTable());

        }

        public HttpServer(int port, Action<IRoutingTable> routes)
            : this("127.0.0.1", port, routes)
        {
        }

        public HttpServer(Action<IRoutingTable> routingTable)
            : this(8080, routingTable)
        {
        }

        public async Task Start()
        {
            this.serverListener.Start();

            Console.WriteLine($"Server started on port {port}");
            Console.WriteLine("Listening for requests ... ");
            while (true)
            {
                var connection = await serverListener.AcceptTcpClientAsync();
                _ = Task.Run(async () =>
                {
                    var networkStream = connection.GetStream();
                    var requestText = await this.ReadRequest(networkStream);
                    Console.WriteLine(requestText);
                    var request = Request.Parse(requestText);
                    var response = routingTable.MatchRequest(request);
                    if (response.PreRenderAction != null)
                        response.PreRenderAction(request, response);
                    AddSession(request, response);


                    await WriteResponse(networkStream, response);
                    connection.Close();
                });

            }


        }
        private async Task WriteResponse(NetworkStream networkStream, Response response)
        {
            var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
            await networkStream.WriteAsync(responseBytes);
        }

        private async Task<string> ReadRequest(NetworkStream networkStream)
        {
            var bufferLingth = 1024;
            var buffer = new byte[bufferLingth];
            var requestBuilder = new StringBuilder();
            do
            {
                var bytesRead = await networkStream.ReadAsync(buffer, 0, bufferLingth);
                requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
            while (networkStream.DataAvailable);

            return requestBuilder.ToString();
        }

        private static void AddSession(Request request, Response response)
        {
            var sessionExists = request.Session
                .ContainsKey(Session.SessionCurrentDateKey);

            if (!sessionExists)
            {
                request.Session[Session.SessionCurrentDateKey]
                    = DateTime.Now.ToString();

                response.Cookies
                    .Add(Session.SessionCookieName, request.Session.Id);
            }
        }
    }
}
