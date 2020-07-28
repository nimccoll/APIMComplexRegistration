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
using ScanSource.Services.Models;
using System;
using System.Threading.Tasks;

namespace ScanSource.Services.APIM
{
    public interface ISubscriptionService
    {
        Task<APIMSubscription> CreateSubscription(string subscriptionName, string scope, Guid primaryKey, Guid secondaryKey, string objectId, string email, string firstName, string lastName);

        Task<UserSubscriptions> GetUserSubscriptions(string emailAddress);
    }
}
