using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace HttpClientService.Helpers
{
    public class HttpParameters
    {
        public string RequestUrl { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Guid Id { get; set; }
        public object Content { get; set; }
        public HttpMethod HttpVerb { get; set; }
    }
}
