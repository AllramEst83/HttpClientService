﻿using HttpClientService.Helpers;
using HttpClientService.HttpServiceHelperMethods;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientService
{
    public class HttpClientService
    {
        public HttpServiceHelpers _httpServiceHelpers { get; }

        public HttpClientService(HttpServiceHelpers httpServiceHelpers)
        {
            _httpServiceHelpers = httpServiceHelpers;
        }       

        //GenericHttpRequest
        public async Task<T> GenericHttpGet<T>(HttpParameters httpParameters)
        {
            string url = httpParameters.Id == Guid.Empty ? httpParameters.RequestUrl : String.Format("{0}/{1}", httpParameters.RequestUrl, httpParameters.Id);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, httpParameters.cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                    return _httpServiceHelpers.DeserializeJsonFromStream<T>(stream);

                var content = await _httpServiceHelpers.StreamToStringAsync(stream);

                throw new CustomApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content
                };
            }
        }

        //PostStreamAsyncQueryString
        public async Task<T> PostStreamAsyncQueryString<T>(HttpParameters parameters)
        {
            string url = parameters.Id == Guid.Empty ? parameters.RequestUrl : String.Format("{0}/{1}", parameters.RequestUrl, parameters.Id);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(parameters.HttpVerb, url))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, parameters.cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                    return _httpServiceHelpers.DeserializeJsonFromStream<T>(stream);

                var content = await _httpServiceHelpers.StreamToStringAsync(stream);
                throw new CustomApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content
                };
            }
        }

        //PostStreamAsync
        public async Task<T> PostStreamAsyncContent<T>(HttpParameters parameters)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(parameters.HttpVerb, parameters.RequestUrl))
            using (var httpContent = _httpServiceHelpers.CreateHttpContent(parameters.Content))
            {
                request.Content = httpContent;

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, parameters.cancellationToken)
                    .ConfigureAwait(true))
                {
                    response.EnsureSuccessStatusCode();

                    Stream streamResponse = await response.Content.ReadAsStreamAsync();

                    return _httpServiceHelpers.DeserializeJsonFromStream<T>(streamResponse);
                }
            }
        }
    }
}
