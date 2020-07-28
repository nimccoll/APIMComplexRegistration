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
    public class SubscriptionProperties
    {
        public string ownerId { get; set; }
        public string scope { get; set; }
        public string displayName { get; set; }
        public string state { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime startDate { get; set; }
        public DateTime expirationDate { get; set; }
        public object endDate { get; set; }
        public DateTime notificationDate { get; set; }
        public object stateComment { get; set; }
    }

    public class UserSubscription
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public SubscriptionProperties properties { get; set; }
    }

    public class UserSubscriptions
    {
        public List<UserSubscription> value { get; set; }
        public int count { get; set; }
    }

}
