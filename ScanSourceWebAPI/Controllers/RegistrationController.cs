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
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Newtonsoft.Json;
using ScanSource.Services.ActiveDirectory;
using ScanSource.Services.APIM;
using ScanSource.Services.Models;
using ScanSource.Services.Storage;
using ScanSourceWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ScanSourceWebAPI.Controllers
{
    public class RegistrationController : ApiController
    {
        private static readonly string CLIENT_ID = "AADAppRegistrationClientID";
        private static readonly string CLIENT_SECRET = "AADAppRegistrationClientSecret";
        private AzureServiceTokenProvider _azureServiceTokenProvider = null;
        private KeyVaultClient _keyVaultClient = null;
        private IRegistrationService _registrationService = null;
        private IAppService _appService = null;
        private ISubscriptionService _subscriptionService = null;

        public RegistrationController()
        {
            // Initialize the KeyVaultClient
            // The AzureServiceTokenProvider allows us to access the KeyVault using the 
            // application's Managed Service Identity
            _azureServiceTokenProvider = new AzureServiceTokenProvider();
            _keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    _azureServiceTokenProvider.KeyVaultTokenCallback));
            SecretBundle clientID = _keyVaultClient.GetSecretAsync(ConfigurationManager.AppSettings["KEYVAULT_ENDPOINT"], CLIENT_ID).Result;
            SecretBundle clientSecret = _keyVaultClient.GetSecretAsync(ConfigurationManager.AppSettings["KEYVAULT_ENDPOINT"], CLIENT_SECRET).Result;
            _registrationService = new RegistrationService(ConfigurationManager.AppSettings["TableStorageConnectionString"]);
            _appService = new AppService(ConfigurationManager.AppSettings["TENANT"], clientID.Value, clientSecret.Value);
            _subscriptionService = new SubscriptionService(ConfigurationManager.AppSettings["APIMUrl"], ConfigurationManager.AppSettings["APIMIdentifier"], ConfigurationManager.AppSettings["APIMKey"]);
        }

        // GET api/<controller>/5
        public async Task<HttpResponseMessage> Get(string id)
        {
            HttpResponseMessage result = new HttpResponseMessage();

            List<KeyValuePair<string, string>> queryString = Request.GetQueryNameValuePairs().ToList();
            string objectId = queryString.FirstOrDefault(q => q.Key == "objectId").Value;
            string email = queryString.FirstOrDefault(q => q.Key == "email").Value;
            string firstName = queryString.FirstOrDefault(q => q.Key == "firstName").Value;
            string lastName = queryString.FirstOrDefault(q => q.Key == "lastName").Value;
            if (string.IsNullOrEmpty(id)
                || string.IsNullOrEmpty(objectId)
                || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(firstName)
                || string.IsNullOrEmpty(lastName))
            {
                result.StatusCode = HttpStatusCode.BadRequest;
                result.Content = new StringContent("Please pass an id, objectId, email, firstName, and lastName on the querystring");
            }
            else
            {
                string partitionKey = id.Substring(0, 1).ToLower();
                OrganizationSubscription subscription = await _registrationService.GetOrganizationSubscription(partitionKey, id);

                if (subscription == null)
                {
                    // Create Azure AD Application Registration for the Organization
                    Guid uniqueId = Guid.NewGuid();
                    Application application = new Application();
                    application.DisplayName = $"AAD - {id} Client Application";
                    application.IdentifierUris = new List<string>();
                    application.IdentifierUris.Add($"https://{ConfigurationManager.AppSettings["TENANT"]}/{uniqueId}");
                    application.PasswordCredentials = new List<PasswordCredential>();
                    var startDate = DateTime.Now;
                    Byte[] bytes = new Byte[32];
                    using (var rand = System.Security.Cryptography.RandomNumberGenerator.Create())
                    {
                        rand.GetBytes(bytes);
                    }
                    string clientSecret = Convert.ToBase64String(bytes);
                    application.PasswordCredentials.Add(new PasswordCredential()
                    {
                        CustomKeyIdentifier = null,
                        StartDate = startDate,
                        EndDate = new DateTime(2299, 12, 31, 5, 0, 0, 0),
                        KeyId = Guid.NewGuid(),
                        Value = clientSecret
                    });
                    application.RequiredResourceAccess = new List<RequiredResourceAccess>();
                    RequiredResourceAccess graphResourceAccess = new RequiredResourceAccess()
                    {
                        ResourceAccess = new List<ResourceAccess>(),
                        ResourceAppId = "00000003-0000-0000-c000-000000000000"
                    };
                    graphResourceAccess.ResourceAccess.Add(new ResourceAccess()
                    {
                        Id = new Guid("37f7f235-527c-4136-accd-4a02d197296e"),
                        Type = "Scope"
                    });
                    graphResourceAccess.ResourceAccess.Add(new ResourceAccess()
                    {
                        Id = new Guid("7427e0e9-2fba-42fe-b0c0-848c9e6a8182"),
                        Type = "Scope"
                    });
                    RequiredResourceAccess apimResourceAccess = new RequiredResourceAccess()
                    {
                        ResourceAccess = new List<ResourceAccess>(),
                        ResourceAppId = "30fe3279-fbb4-4a13-b1f8-7c5f2ea9e6df"
                    };
                    apimResourceAccess.ResourceAccess.Add(new ResourceAccess()
                    {
                        Id = new Guid("f9bcce35-145a-4199-bf1b-948467774061"),
                        Type = "Role"
                    });
                    application.RequiredResourceAccess.Add(graphResourceAccess);
                    application.RequiredResourceAccess.Add(apimResourceAccess);
                    application.ReplyUrls = new List<string>();
                    application.ReplyUrls.Add($"msapp://{uniqueId}");
                    string clientId = await _appService.Create(application);

                    // Create APIM subscription key for the organization
                    Guid primaryKey = Guid.NewGuid();
                    Guid secondaryKey = Guid.NewGuid();
                    APIMSubscription apimSubscription = await _subscriptionService.CreateSubscription($"APIM {id} Subscription", "/products/starter", primaryKey, secondaryKey, objectId, email, firstName, lastName);

                    // Store subscription information in Table Storage
                    OrganizationSubscription organizationSubscription = new OrganizationSubscription()
                    {
                        Organization = id,
                        PrimarySubscriptionKey = apimSubscription.properties.primaryKey,
                        SecondarySubscriptionKey = apimSubscription.properties.secondaryKey,
                        Scope = apimSubscription.properties.scope,
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    };
                    OrganizationEntity organizationEntity = new OrganizationEntity(organizationSubscription);
                    await _registrationService.CreateOrganizationSubscription(organizationEntity);

                    ResponseContent responseContent = new ResponseContent
                    {
                        version = "1.0.0",
                        status = (int)HttpStatusCode.OK,
                        primarySubscriptionKey = apimSubscription.properties.primaryKey,
                        secondarySubscriptionKey = apimSubscription.properties.secondaryKey,
                        clientId = clientId,
                        clientSecret = clientSecret
                    };
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(responseContent), Encoding.UTF8, "application/json");
                }
                else
                {
                    bool userHasSubscription = false;
                    UserSubscriptions userSubscriptions = await _subscriptionService.GetUserSubscriptions(email);
                    if (userSubscriptions != null && userSubscriptions.count > 0)
                    {
                        foreach (UserSubscription userSubscription in userSubscriptions.value)
                        {
                            if (userSubscription.properties.scope.EndsWith(subscription.Scope, StringComparison.InvariantCultureIgnoreCase))
                            {
                                userHasSubscription = true;
                                break;
                            }
                        }
                    }

                    if (!userHasSubscription)
                    {
                        APIMSubscription apimSubscription = await _subscriptionService.CreateSubscription($"APIM {id} Subscription", "/products/starter", new Guid(subscription.PrimarySubscriptionKey), new Guid(subscription.SecondarySubscriptionKey), objectId, email, firstName, lastName);
                    }

                    ResponseContent responseContent = new ResponseContent
                    {
                        version = "1.0.0",
                        status = (int)HttpStatusCode.OK,
                        primarySubscriptionKey = subscription.PrimarySubscriptionKey,
                        secondarySubscriptionKey = subscription.SecondarySubscriptionKey,
                        clientId = subscription.ClientId,
                        clientSecret = subscription.ClientSecret
                    };
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(responseContent), Encoding.UTF8, "application/json");
                }
            }

            return result;
        }
    }
}