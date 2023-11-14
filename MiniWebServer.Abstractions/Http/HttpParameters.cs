namespace MiniWebServer.Abstractions.Http
{
    public class HttpParameters : Dictionary<string, HttpParameter>
    {
        public HttpParameters(params HttpParameter[] httpParameter)
        {
            foreach (var item in httpParameter)
            {
                if (TryGetValue(item.Name, out var value))
                {
                    value.AddValue(item.Value ?? string.Empty);
                }
                else
                {
                    Add(item.Name, item);
                }
            }
        }

        public HttpParameters(HttpParameters others)
        {
            foreach (var item in others.Values)
            {
                if (TryGetValue(item.Name, out var value))
                {
                    value.AddValue(item.Value ?? string.Empty);
                }
                else
                {
                    Add(item.Name, item);
                }
            }
        }


        public HttpParameters(IEnumerable<HttpParameter> collection)
        {
            foreach (var item in collection)
            {
                if (TryGetValue(item.Name, out var value))
                {
                    value.AddValue(item.Value ?? string.Empty);
                }
                else
                {
                    Add(item.Name, item);
                }
            }
        }

        public void Add(HttpParameter item)
        {
            if (TryGetValue(item.Name, out var value))
            {
                value.AddValue(item.Value ?? string.Empty);
            }
            else
            {
                Add(item.Name, item);
            }
        }
    }
}
