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
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScanSource.Services.ActiveDirectory
{
    public class AppService : IAppService
    {
        private ActiveDirectoryClient _activeDirectoryClient;

        private AppService()
        {
        }

        public AppService(string tenant, string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(tenant)) throw new ArgumentNullException("tenant");
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException("clientId");
            if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException("clientSecret");

            AuthenticationContext authenticationContext = new AuthenticationContext($"{Constants.Authority}{tenant}", false);
            ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);
            AuthenticationResult authenticationResult = authenticationContext.AcquireTokenAsync(Constants.ResourceUrl, clientCredential).Result;
            string accessToken = authenticationResult.AccessToken;

            Uri serviceRoot = new Uri($"{Constants.ResourceUrl}/{tenant}");
            _activeDirectoryClient = new ActiveDirectoryClient(serviceRoot, async () => await Task.Run(() => { return accessToken; }));
        }

        public List<IApplication> List(int pageNumber, out bool moreAvailable)
        {
            IPagedCollection<IApplication> applicationList = null;
            List<IApplication> applications = null;

            moreAvailable = false;
            applicationList = _activeDirectoryClient.Applications.ExecuteAsync().Result;

            IPagedCollection<IServicePrincipal> servicePrincipals = _activeDirectoryClient.ServicePrincipals.Where(s => s.AppId == "00000002-0000-0000-c000-000000000000").ExecuteAsync().Result;

            if (applicationList != null)
            {
                applications = applicationList.CurrentPage.ToList();
                moreAvailable = applicationList.MorePagesAvailable;
                if (pageNumber > 1)
                {
                    for (int i = 1; i < pageNumber; i++)
                    {
                        if (moreAvailable)
                        {
                            applicationList = applicationList.GetNextPageAsync().Result;
                            if (applicationList != null)
                            {
                                applications = applicationList.CurrentPage.ToList();
                                moreAvailable = applicationList.MorePagesAvailable;
                            }
                            else
                            {
                                applications = new List<IApplication>();
                            }
                        }
                    }
                }
            }
            else
            {
                applications = new List<IApplication>();
            }

            return applications.OrderBy(a => a.DisplayName).ToList();
        }

        public async Task<string> Create(Application application)
        {
            string applicationId = string.Empty;

            await _activeDirectoryClient.Applications.AddApplicationAsync(application);
            if (application != null)
            {
                applicationId = application.AppId;
                ServicePrincipal servicePrincipal = new ServicePrincipal();
                servicePrincipal.DisplayName = application.DisplayName;
                servicePrincipal.AccountEnabled = true;
                servicePrincipal.AppId = application.AppId;
                await _activeDirectoryClient.ServicePrincipals.AddServicePrincipalAsync(servicePrincipal);
            }

            return applicationId;
        }

        public List<IServicePrincipal> SearchServicePrincipals(string displayName, int pageNumber, out bool moreAvailable)
        {
            IPagedCollection<IServicePrincipal> servicePrincipalList = null;
            List<IServicePrincipal> servicePrincipals = null;

            moreAvailable = false;
            servicePrincipalList = _activeDirectoryClient.ServicePrincipals.Where(s => s.DisplayName.StartsWith(displayName)).ExecuteAsync().Result;

            if (servicePrincipalList != null)
            {
                servicePrincipals = servicePrincipalList.CurrentPage.ToList();
                moreAvailable = servicePrincipalList.MorePagesAvailable;
                if (pageNumber > 1)
                {
                    for (int i = 1; i < pageNumber; i++)
                    {
                        if (moreAvailable)
                        {
                            servicePrincipalList = servicePrincipalList.GetNextPageAsync().Result;
                            if (servicePrincipalList != null)
                            {
                                servicePrincipals = servicePrincipalList.CurrentPage.ToList();
                                moreAvailable = servicePrincipalList.MorePagesAvailable;
                            }
                            else
                            {
                                servicePrincipals = new List<IServicePrincipal>();
                            }
                        }
                    }
                }
            }
            else
            {
                servicePrincipals = new List<IServicePrincipal>();
            }

            return servicePrincipals;
        }

        public IServicePrincipal GetServicePrincipal(string appId)
        {
            IServicePrincipal servicePrincipal = null;

            servicePrincipal = _activeDirectoryClient.ServicePrincipals.Where(s => s.AppId == appId).ExecuteSingleAsync().Result;
            return servicePrincipal;
        }
    }

    internal class Constants
    {
        public const string ResourceUrl = "https://graph.windows.net";
        public const string Authority = "https://login.microsoftonline.com/";
    }
}