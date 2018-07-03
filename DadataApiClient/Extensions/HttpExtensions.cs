﻿

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DadataApiClient.Exceptions;
using DadataApiClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DadataApiClient.Extensions
{
    public static class HttpExtensions
    {
        public static async Task<TResponse> SendResponseAsync<TResponse>(this HttpClient client, HttpMethod method,
            Uri url, JObject value) where TResponse : class =>
            await SendResponseAsync<TResponse>(method, url, value, client);
        
        public static async Task<TResponse> SendResponseAsync<TResponse>(this HttpClient client, HttpMethod method,
            Uri url, JArray value) where TResponse : class =>
            await SendResponseAsync<TResponse>(method, url, value, client);
        
        private static async Task<TResponse> SendResponseAsync<TResponse>(HttpMethod method, Uri url, object value, HttpClient client) where TResponse : class
        {
            var httpRequestMessage = new HttpRequestMessage(method, url);

            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8,
                "application/json");
            
            using (HttpResponseMessage response = await client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead))
            {
                var result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                    return JsonConvert.DeserializeObject<TResponse>(result, new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        },
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                        Formatting = Formatting.Indented
                    });
                throw new BadRequestException($"{response.StatusCode} {response.ReasonPhrase}");
            }
        }
    }
}