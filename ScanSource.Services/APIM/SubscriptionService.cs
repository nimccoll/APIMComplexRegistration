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
using Newtonsoft.Json;
using ScanSource.Services.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ScanSource.Services.APIM
{
    public class SubscriptionService : ISubscriptionService
    {
        private string _apimUrl = string.Empty;
        private string _apimIdentifier = string.Empty;
        private string _apimKey = string.Empty;
        private HttpClient _httpClient;

        private SubscriptionService()
        {
        }

        public SubscriptionService(string apimUrl, string apimIdentifier, string apimKey)
        {
            if (string.IsNullOrEmpty(apimUrl)) throw new ArgumentNullException("apimUrl");
            if (string.IsNullOrEmpty(apimIdentifier)) throw new ArgumentNullException("apimIdentifier");
            if (string.IsNullOrEmpty(apimKey)) throw new ArgumentNullException("apimKey");

            _apimUrl = apimUrl;
            _apimIdentifier = apimIdentifier;
            _apimKey = apimKey;
            _httpClient = new HttpClient();
        }

        public async Task<APIMSubscription> CreateSubscription(string displayName, string scope, Guid primaryKey, Guid secondaryKey, string objectId, string email, string firstName, string lastName)
        {
            APIMSubscription apimSubscription = null;
            string accessToken = GenerateAccessToken();
            User user = await GetUser(email, accessToken);
            if (user == null)
            {
                user = await CreateUser(accessToken, objectId, email, firstName, lastName);
            }
            string userId = $"/users/{user.name}";
            string subscriptionName = displayName.Replace(" ", "-").ToLower();
            string url = $"{_apimUrl}/subscriptions/{subscriptionName}?api-version=2019-12-01";
            string requestBody = "{ \"properties\": { \"primaryKey\": \"" + primaryKey + "\", \"scope\": \"" + scope + "\", \"secondaryKey\": \"" + secondaryKey + "\", \"displayName\": \"" + displayName + "\", \"ownerId\": \"" + userId + "\" } }";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", accessToken);
            HttpResponseMessage response = await _httpClient.PutAsync(url, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                apimSubscription = JsonConvert.DeserializeObject<APIMSubscription>(responseContent);
                apimSubscription.properties.scope = scope;
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
            }

            return apimSubscription;
        }

        public async Task<UserSubscriptions> GetUserSubscriptions(string emailAddress)
        {
            return await GetUserSubscriptions(emailAddress, GenerateAccessToken());
        }

        private async Task<User> GetUser(string emailAddress, string accessToken)
        {
            User user = null;

            string url = $"{_apimUrl}/users?api-version=2019-12-01&$filter=email eq '{emailAddress}'";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", accessToken);
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Users users = JsonConvert.DeserializeObject<Users>(responseContent);
                if (users.count == 1)
                {
                    user = users.value[0];
                }
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
            }

            return user;
        }

        private async Task<User> CreateUser(string accessToken, string objectId, string emailAddress, string firstName, string lastName)
        {
            User user = null;
            UserProperties properties = new UserProperties()
            {
                email = emailAddress,
                firstName = firstName,
                lastName = lastName,
                identities = new List<Identity>()
                {
                    new Identity()
                    {
                        id = objectId,
                        provider = "AadB2C"
                    }
                }
            };

            string url = $"{_apimUrl}/users/{Guid.NewGuid().ToString()}?api-version=2019-12-01";
            User newUser = new User()
            {
                properties = properties
            };
            string requestBody = JsonConvert.SerializeObject(newUser);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", accessToken);
            HttpResponseMessage response = await _httpClient.PutAsync(url, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<User>(responseContent);
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
            }

            return user;
        }

        private async Task<UserSubscriptions> GetUserSubscriptions(string emailAddress, string accessToken)
        {
            UserSubscriptions userSubscriptions = null;

            User user = await GetUser(emailAddress, accessToken);
            if (user != null)
            {
                string url = $"{_apimUrl}/users/{user.name}/subscriptions?api-version=2019-12-01";
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", accessToken);
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    userSubscriptions = JsonConvert.DeserializeObject<UserSubscriptions>(responseContent);
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                }
            }

            return userSubscriptions;
        }

        private string GenerateAccessToken()
        {
            string accessToken = string.Empty;

            var expiry = DateTime.UtcNow.AddDays(10);
            using (HMACSHA512 encoder = new HMACSHA512(Encoding.UTF8.GetBytes(_apimKey)))
            {
                string dataToSign = _apimIdentifier + "\n" + expiry.ToString("O", CultureInfo.InvariantCulture);
                byte[] hash = encoder.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                string signature = Convert.ToBase64String(hash);
                accessToken = string.Format("uid={0}&ex={1:o}&sn={2}", _apimIdentifier, expiry, signature);
            }

            return accessToken;
        }
    }
}
