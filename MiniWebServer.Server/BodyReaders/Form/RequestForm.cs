using Microsoft.Extensions.Primitives;
using MiniWebServer.MiniApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.BodyReaders.Form
{
    public class RequestForm : IRequestForm
    {
        private readonly Dictionary<string, StringValues> form;

        public RequestForm()
        {
            form = new Dictionary<string, StringValues>();
        }

        public RequestForm(Dictionary<string, StringValues> form)
        {
            this.form = form ?? new Dictionary<string, StringValues>();
        }

        public StringValues this[string key]
        {
            get
            {
                if (form.TryGetValue(key, out StringValues values)) {  
                    return values; 
                }
                else
                {
                    return StringValues.Empty;
                }
            }

            set
            {
                if (form.TryGetValue(key, out StringValues values))
                {
                    var newValues = new List<string>(values);
                    newValues.AddRange(value);

                    form[key] = new StringValues(newValues.ToArray());
                }
                else
                {
                    form[key] = value;
                }
            }
        }

        public int Count => form.Count;

        public ICollection<string> Keys => form.Keys;

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
        {
            return form.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return form.GetEnumerator();
        }

    }
}
