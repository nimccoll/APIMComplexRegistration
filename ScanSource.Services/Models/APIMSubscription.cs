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
using System;
using System.Collections.Generic;

namespace ScanSource.Services.Models
{
    public class Owner
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string state { get; set; }
        public DateTime registrationDate { get; set; }
        public object note { get; set; }
        public List<object> groups { get; set; }
        public List<object> identities { get; set; }
    }

    public class Properties
    {
        public string ownerId { get; set; }
        public Owner user { get; set; }
        public string scope { get; set; }
        public string displayName { get; set; }
        public string state { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? expirationDate { get; set; }
        public object endDate { get; set; }
        public DateTime? notificationDate { get; set; }
        public string primaryKey { get; set; }
        public string secondaryKey { get; set; }
        public object stateComment { get; set; }
    }

    public class APIMSubscription
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public Properties properties { get; set; }
    }
}
