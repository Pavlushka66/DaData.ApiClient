﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DaData.Converters;
using DaData.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DaData.Http
{
    public static class HttpExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        private static readonly JsonSerializerSettings DeserializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Converters = {new TimestampToDateTimeConverter()},
            Formatting = Formatting.Indented
        };
        
        public static Uri AddQueryParameters(this System.Uri uri, IEnumerable<KeyValuePair<string, object>> queryParameters)
        {
            var builder = new StringBuilder(uri.ToString());
            if (queryParameters != null)
            {
                var parameters = queryParameters.ToList();
                var first = parameters.First();
                builder.Append($"{(string.IsNullOrEmpty(uri.Query) ? '?': '&')}{first.Key}={first.Value}");
                
                foreach (var parameter in parameters.Skip(1))
                {
                    builder.Append($"&{parameter.Key}={parameter.Value}");
                }
            }

            return new Uri(builder.ToString());
        }

        /// <summary>
        /// Extended method for creation a query string
        /// </summary>
        /// <param name="uriString"></param>
        /// <param name="queryParameters"></param>
        /// <returns></returns>
        public static string AddQueryParameters(this string uriString,
            IEnumerable<KeyValuePair<string, object>> queryParameters) =>
            AddQueryParameters(new System.Uri(uriString), queryParameters).ToString();
        
        /// <summary>
        /// Send http request to DaData api
        /// </summary>
        /// <param name="client">Instance of HttpClient</param>
        /// <param name="method">Http method</param>
        /// <param name="uri">Uri(can have query string parameters set via the constructor or just in url)</param>
        /// <param name="value">Object for request</param>
        /// <typeparam name="TResponse">type of returned object</typeparam>
        /// <returns></returns>
        /// <exception cref="PaymentRequiredException"></exception>
        /// <exception cref="KeyIsNotExistException"></exception>
        /// <exception cref="MethodNotAllowedException"></exception>
        /// <exception cref="TooManyRequestsPerSecondException"></exception>
        /// <exception cref="BadRequestException"></exception>
        public static async Task<TResponse> SendResponseAsync<TResponse>(this HttpClient client, HttpMethod method, Uri uri, object value = null) where TResponse : class
        {
            var httpRequestMessage = new HttpRequestMessage(method, uri);
            
            if(value != null)
                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(value, SerializerSettings), Encoding.UTF8,
                    "application/json");

            using (HttpResponseMessage response = await client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
            {
                var result = await response.Content.ReadAsStringAsync();
                
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return JsonConvert.DeserializeObject<TResponse>(string.IsNullOrEmpty(result) ? null : result, DeserializerSettings);
                    case HttpStatusCode.PaymentRequired:
                        throw new PaymentRequiredException();
                    case HttpStatusCode.Forbidden:
                        throw new KeyIsNotExistException();
                    case HttpStatusCode.MethodNotAllowed:
                        throw new MethodNotAllowedException();
                    case HttpStatusCode.RequestEntityTooLarge:
                        throw new TooManyRequestsPerSecondException();
                    default:
                        throw new BadRequestException($"{response.StatusCode.ToString()} {result}");
                } 
            }
        }
    }
}