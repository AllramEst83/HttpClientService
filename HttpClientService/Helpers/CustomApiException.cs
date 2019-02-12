using System;
using System.Collections.Generic;
using System.Text;

namespace HttpClientService.Helpers
{
    public class CustomApiException : Exception
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
    }
}
