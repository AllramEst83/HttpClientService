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
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        Type t = typeof(T);
                        object[] args = new object[] { 403 };
                        T o = (T)Activator.CreateInstance(t, args);

                        return o;

                        //var content = await StreamToStringAsync(stream);
                        //throw new CustomApiException
                        //{
                        //    StatusCode = (int)response.StatusCode,
                        //    Content = content
                        //};

                    }
                }

                return DeserializeJsonFromStream<T>(stream);
            }
        }

        //PostStreamAsyncQueryString
        public static async Task<T> PostStreamAsyncQueryString<T>(HttpParameters parameters)
        {
            string url = String.IsNullOrEmpty(parameters.Id) ? parameters.RequestUrl : String.Format("{0}/{1}", parameters.RequestUrl, parameters.Id);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(parameters.HttpVerb, url))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, parameters.CancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                    return DeserializeJsonFromStream<T>(stream);

                var content = await StreamToStringAsync(stream);
                throw new CustomApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content
                };
            }
        }

        //PostStreamAsync
        public static async Task<T> PostStreamAsyncContent<T>(HttpParameters parameters)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(parameters.HttpVerb, parameters.RequestUrl))
            using (var httpContent = CreateHttpContent(parameters.Content))
            {
                request.Content = httpContent;

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, parameters.CancellationToken)
                    .ConfigureAwait(true))
                {
                    response.EnsureSuccessStatusCode();
                    Stream streamResponse = await response.Content.ReadAsStreamAsync();
                    return DeserializeJsonFromStream<T>(streamResponse);

                    throw new CustomApiException
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = streamResponse.ToString()
                    };
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
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }
    }
}
