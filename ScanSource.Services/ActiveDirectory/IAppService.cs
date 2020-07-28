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
using Microsoft.Azure.ActiveDirectory.GraphClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScanSource.Services.ActiveDirectory
{
    public interface IAppService
    {
        List<IApplication> List(int pageNumber, out bool moreAvailable);
        Task<string> Create(Application application);
        List<IServicePrincipal> SearchServicePrincipals(string displayName, int pageNumber, out bool moreAvailable);
        IServicePrincipal GetServicePrincipal(string appId);
    }
}
