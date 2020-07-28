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
using System.Threading.Tasks;

namespace ScanSource.Services.Storage
{
    public interface IRegistrationService
    {
        Task<OrganizationSubscription> GetOrganizationSubscription(string partitionKey, string rowKey);

        Task CreateOrganizationSubscription(OrganizationEntity organizationEntity);
    }
}
