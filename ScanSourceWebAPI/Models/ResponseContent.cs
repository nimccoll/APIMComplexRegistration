//===============================================================================
// Microsoft FastTrack for Azure
// APIM Complex Registration POC
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
namespace ScanSourceWebAPI.Models
{
    public class ResponseContent
    {
        public string version { get; set; }
        public int status { get; set; }
        public string organization { get; set; }
        public string primarySubscriptionKey { get; set; }
        public string secondarySubscriptionKey { get; set; }
        public string clientId { get; set; }
        public string clientSecret { get; set; }
    }
}