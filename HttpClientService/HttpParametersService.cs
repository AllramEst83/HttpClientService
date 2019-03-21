using HttpClientService.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace HttpClientService
{
    public static class HttpParametersService
    {
        //GetHttpParameters
        public static HttpParameters GetHttpParameters(object model, string requestUrl, HttpMethod httpVerb, string id, string jwtToken = "")
        {
            HttpParameters httpParameters =
              new HttpParameters
              {
                  Content = model,
                  HttpVerb = httpVerb,
                  RequestUrl = requestUrl,
                  Id = id,
                  CancellationToken = CancellationToken.None,
                  JwtToken = jwtToken
              };

            return httpParameters;
        }
    }
}
