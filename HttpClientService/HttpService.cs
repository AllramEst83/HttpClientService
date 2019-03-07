using HttpClientService.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientService
{
    public static class HttpService
    {
        //REF: https://johnthiriet.com/efficient-api-calls/
        //REF-GIT-Source: https://github.com/johnthiriet/EfficientHttpClient

        //Build a .nupkg from the solution folder in in the command line with 'dotnet pack --no-build'



        //GenericHttpRequest
        public static async Task<T> GenericHttpGet<T>(HttpParameters httpParameters)
        {
            string url = String.IsNullOrEmpty(httpParameters.Id) ? httpParameters.RequestUrl : String.Format("{0}?userId={1}", httpParameters.RequestUrl, httpParameters.Id);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (httpParameters.JwtToken != String.Empty)
                {
                    client.DefaultRequestHeaders.Add(HttpConstants.Authorization, httpParameters.JwtToken);
                }

                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, httpParameters.CancellationToken);

                var stream = await response.Content.ReadAsStreamAsync();

                if (!response.IsSuccessStatusCode)
                {
                    T newObject = NewObjectInstanceGenerator<T>(response.StatusCode);

                    return newObject;

                    //var content = await StreamToStringAsync(stream);
                    //throw new CustomApiException
                    //{
                    //    StatusCode = (int)response.StatusCode,
                    //    Content = content
                    //};



                }

                return DeserializeJsonFromStream<T>(stream);
            }
        }

        //PostStreamAsyncQueryString
        public static async Task<T> PostStreamAsyncQueryString<T>(HttpParameters httpParameters)
        {
            string url = String.IsNullOrEmpty(httpParameters.Id) ? httpParameters.RequestUrl : String.Format("{0}/{1}", httpParameters.RequestUrl, httpParameters.Id);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(httpParameters.HttpVerb, url))
            {
                if (httpParameters.JwtToken != String.Empty)
                {
                    client.DefaultRequestHeaders.Add(HttpConstants.Authorization, httpParameters.JwtToken);
                }

                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, httpParameters.CancellationToken);

                var stream = await response.Content.ReadAsStreamAsync();

                if (!response.IsSuccessStatusCode)
                {
                    T newObject = NewObjectInstanceGenerator<T>(response.StatusCode);

                    return newObject;
                }

                //var content = await StreamToStringAsync(stream);
                //throw new CustomApiException
                //{
                //    StatusCode = (int)response.StatusCode,
                //    Content = content
                //};

                return DeserializeJsonFromStream<T>(stream);
            }
        }

        //PostStreamAsync
        public static async Task<T> PostStreamAsyncContent<T>(HttpParameters httpParameters)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(httpParameters.HttpVerb, httpParameters.RequestUrl))
            using (var httpContent = CreateHttpContent(httpParameters.Content))
            {
                request.Content = httpContent;

                if (httpParameters.JwtToken != String.Empty)
                {
                    client.DefaultRequestHeaders.Add(HttpConstants.Authorization, httpParameters.JwtToken);
                }

                using (var response = await client
                    .SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    httpParameters.CancellationToken)
                    .ConfigureAwait(true))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        T newObject = NewObjectInstanceGenerator<T>(response.StatusCode);

                        return newObject;
                    }

                    Stream streamResponse = await response.Content.ReadAsStreamAsync();

                    return DeserializeJsonFromStream<T>(streamResponse); ;

                    //throw new CustomApiException
                    //{
                    //    StatusCode = (int)response.StatusCode,
                    //    Content = streamResponse.ToString()
                    //};
                }
            }
        }


        //DeserializeJsonFromStream
        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var jr = new JsonSerializer();
                var searchResult = jr.Deserialize<T>(jtr);
                return searchResult;
            }
        }

        //StreamToStringAsync
        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    content = await sr.ReadToEndAsync();
                }
            }

            return content;
        }

        //SerializeJsonIntoStream
        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, value);
                jtw.Flush();
            }
        }

        //CreateHttpContent
        private static HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(HttpConstants.MediaContentType_APPLICATION_JSON);
            }

            return httpContent;
        }

        private static T NewObjectInstanceGenerator<T>(System.Net.HttpStatusCode statusCode)
        {
            object[] args = null;
            switch (statusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    args = new object[]
                    {
                        HttpConstants.UnauthorizedStatusCode ,
                        HttpConstants.UnauthorizedError,
                        HttpConstants.UnauthorizedDescription,
                        HttpConstants.UnauthorizedCode
                    };

                    break;

                case System.Net.HttpStatusCode.Forbidden:

                    args = new object[]
                    {
                        HttpConstants.ForbiddenStatusCode,
                        HttpConstants.ForbiddenError,
                        HttpConstants.ForbiddenDescription,
                        HttpConstants.ForbiddenCode
                    };
                    break;

                default:
                    args = new object[]
                  {
                        500,
                        HttpConstants.ForbiddenStatusCode,
                        HttpConstants.InternalServerError_Error,
                        HttpConstants.InternalServerErrorCode
                  };
                    break;
            }

            Type t = typeof(T);
            T objectInstance = (T)Activator.CreateInstance(t, args);

            return objectInstance;
        }
    }
}
