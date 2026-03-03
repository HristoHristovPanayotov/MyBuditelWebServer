using System.Collections;

namespace WebServer.Server.HTTP_Request
{
    public class HeaderCollection : IEnumerable<Header>
    {
        private readonly Dictionary<string, Header> headers;
        public HeaderCollection()
        {
            headers = new Dictionary<string, Header>();
        }

        public string this[string name]
        {
            get
            {
                return headers[name].Value;
            }
            set
            {
                headers[name].Value = value;
            }
        }

        public int Count => this.headers.Count;

        public bool Contains(string name)
        {
            return this.headers.ContainsKey(name);
        }

        public void Add(string name, string value)
        {

            var headers = new Header(name, value);
            this.headers[name] = headers;
        }

        public IEnumerator<Header> GetEnumerator()
        {
            return headers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
