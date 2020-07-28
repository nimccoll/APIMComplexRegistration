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
using ScanSource.Services.Models;
using System;
using System.Threading.Tasks;

namespace ScanSource.Services.Storage
{
    public class RegistrationService : IRegistrationService
    {
        private static readonly string TABLE_NAME = "ScanSourceSubscriptions";
        private CloudStorageAccount _cloudStorageAccount = null;
        private CloudTableClient _cloudTableClient = null;

        private RegistrationService()
        {
        }

        public RegistrationService(string tableStorageConnectionString)
        {
            if (string.IsNullOrEmpty(tableStorageConnectionString)) throw new ArgumentNullException("tableStorageConnectionString");

            _cloudStorageAccount = CloudStorageAccount.Parse(tableStorageConnectionString);
            _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
        }

        public async Task<OrganizationSubscription> GetOrganizationSubscription(string partitionKey, string rowKey)
        {
            OrganizationSubscription subscription = null;
            CloudTable scanSourceSubscriptions = _cloudTableClient.GetTableReference(TABLE_NAME);
            bool exists = await scanSourceSubscriptions.ExistsAsync();

            if (exists)
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<OrganizationEntity>(partitionKey, rowKey);
                TableResult result = await scanSourceSubscriptions.ExecuteAsync(retrieveOperation);
                OrganizationEntity orgEntity = result.Result as OrganizationEntity;
                if (orgEntity != null)
                {
                    subscription = orgEntity.Subscription;
                }
            }

            return subscription;
        }

        public async Task CreateOrganizationSubscription(OrganizationEntity organizationEntity)
        {
            CloudTable scanSourceSubscriptions = _cloudTableClient.GetTableReference(TABLE_NAME);
            bool exists = await scanSourceSubscriptions.ExistsAsync();

            if (exists)
            {
                TableOperation createOperation = TableOperation.Insert(organizationEntity);
                TableResult result = await scanSourceSubscriptions.ExecuteAsync(createOperation);
            }
        }
    }
}
