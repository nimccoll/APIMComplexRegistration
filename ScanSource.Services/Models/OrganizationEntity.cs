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
using Microsoft.Azure.Cosmos.Table;

namespace ScanSource.Services.Models
{
    public class OrganizationEntity : TableEntity
    {
        public OrganizationEntity()
        {
        }

        public OrganizationEntity(OrganizationSubscription subscription)
        {
            this.PartitionKey = subscription.Organization.Substring(0, 1).ToLower();
            this.RowKey = subscription.Organization;
            this.PrimarySubscriptionKey = subscription.PrimarySubscriptionKey;
            this.SecondarySubscriptionKey = subscription.SecondarySubscriptionKey;
            this.Scope = subscription.Scope;
            this.ClientId = subscription.ClientId;
            this.ClientSecret = subscription.ClientSecret;
        }
        public string PrimarySubscriptionKey { get; set; }

        public string SecondarySubscriptionKey { get; set; }

        public string Scope { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public OrganizationSubscription Subscription
        {
            get
            {
                return new OrganizationSubscription()
                {
                    Organization = this.RowKey,
                    PrimarySubscriptionKey = this.PrimarySubscriptionKey,
                    SecondarySubscriptionKey = this.SecondarySubscriptionKey,
                    Scope = this.Scope,
                    ClientId = this.ClientId,
                    ClientSecret = this.ClientSecret
                };
            }
        }
    }
}
