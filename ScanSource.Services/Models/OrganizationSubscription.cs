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
namespace ScanSource.Services.Models
{
    public class OrganizationSubscription
    {
        public string Organization { get; set; }

        public string PrimarySubscriptionKey { get; set; }

        public string SecondarySubscriptionKey { get; set; }

        public string Scope { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
