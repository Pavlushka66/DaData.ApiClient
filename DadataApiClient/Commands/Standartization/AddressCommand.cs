﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DadataApiClient.Commands.Base;
using DadataApiClient.Exceptions;
using DadataApiClient.Extensions;
using DadataApiClient.Models;
using DadataApiClient.Models.Standartization.Responses;
using Newtonsoft.Json.Linq;

namespace DadataApiClient.Commands.Standartization
{
    public class AddressCommand : StandartizationCommandBase
    {
        public AddressCommand()
        {
            Url = "https://dadata.ru/api/v2/clean/address";
        }

        public override async Task<BaseResponse> Execute(object query, HttpClient client)
        {
            if(!(query is List<string> temp) || temp.Count == 0)
                throw new InvalidQueryException(query);
            
            var value = new JArray(temp);
            
            return await client.SendResponseAsync<DadataAddressQueryBaseResponse>(HttpMethod.Post, new Uri(Url), value);
        }
    }
}