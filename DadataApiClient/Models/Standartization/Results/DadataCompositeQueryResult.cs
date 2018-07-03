﻿using System.Collections.Generic;
using DadataApiClient.Models.Standartization.Data;

namespace DadataApiClient.Models.Standartization.Results
{
    public class DadataCompositeQueryResult
    {
        public List<string> Structure { get; set; }

        public List<DadataDataQueryData> Data { get; set; }
    }
}