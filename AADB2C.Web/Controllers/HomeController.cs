//===============================================================================
// Microsoft FastTrack for Azure
// Azure Active Directory B2C Authentication Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using AADB2C.Web.Models;
using CalcModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AADB2C.Web.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration = null;
        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Constructor overload that will allow access to the application configuration settings.
        /// An instance of the IConfiguration interface will be injected by the ASP.Net Core dependency
        /// injection framework.
        /// </summary>
        /// <param name="configuration"><see cref="Microsoft.Extensions.Configuration.IConfiguration"/></param>
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            List<Claim> claims = new List<Claim>();
            if (User.Identity.IsAuthenticated)
            {
                foreach (Claim claim in User.Claims)
                {
                    claims.Add(claim);
                }
                ViewBag.Claims = claims;
            }

            return View();
        }

        /// <summary>
        /// Calls a REST API on the APIM gateway that executes a simple calculation. This method requires an 
        /// authenticated user.
        /// </summary>
        /// <returns><see cref="Microsoft.AspNetCore.Mvc.IActionResult"/> A view that displays the values returned by the API.</returns>
        [Authorize]
        public async Task<IActionResult> CallAPI()
        {
            // Retrieve an access token for the WebAPI
            string accessToken = await GetAPIAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                // If an empty access token is returned, acquistion of the access token for the APIM
                // gateway via Client Credentials failed, redirect to the home page.
                return RedirectToAction("Index");
            }
            else
            {
                InputModel model = new InputModel();
                model.IntegerInput = new int[] { 23, 66 };
                model.StringInput = new string[] { "Add", "Integer" };

                HttpClient httpClient = new HttpClient();
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _configuration.GetValue<string>("AzureADB2C:ApiUrl")))
                {
                    // Set the authorization header
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    // Set the subscription key
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _configuration.GetValue<string>("APIMSubscriptionKey"));
                    string content = JsonConvert.SerializeObject(model);
                    request.Content = new StringContent(content, Encoding.UTF8, "application/json");

                    // Send the request to APIM endpoint
                    using (HttpResponseMessage response = await httpClient.SendAsync(request))
                    {
                        string error = await response.Content.ReadAsStringAsync();

                        // Check the result for error
                        if (!response.IsSuccessStatusCode)
                        {
                            // Throw server busy error message
                            if (response.StatusCode == (HttpStatusCode)429)
                            {
                                // TBD: Add you error handling here
                            }

                            throw new Exception(error);
                        }

                        // Return the response body, usually in JSON format
                        OutputModel outputModel = JsonConvert.DeserializeObject<OutputModel>(await response.Content.ReadAsStringAsync());
                        ViewBag.Operation = model.StringInput[0];
                        if (model.StringInput[1] == "Integer")
                        {
                            ViewBag.Calc1Input1 = model.IntegerInput[0];
                            ViewBag.Calc1Input2 = model.IntegerInput[1];
                            ViewBag.Calc1Result = outputModel.IntegerOutput[0];
                        }
                        else
                        {
                            ViewBag.Calc1Input1 = model.DecimalInput[0];
                            ViewBag.Calc1Input2 = model.DecimalInput[1];
                            ViewBag.Calc1Result = outputModel.DecimalOutput[0];
                        }
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Use the client ID and client secret assigned to the user's organization to retrieve an 
        /// access token for the APIM gateway using the Client Credential flow.
        /// </summary>
        /// <returns>WebAPI access token. Empty if token acquisition failed.</returns>
        private async Task<string> GetAPIAccessToken()
        {
            string tenant = _configuration.GetValue<string>("AzureADB2C:Domain");
            string clientId = _configuration.GetValue<string>("AzureAD:ClientId");
            string clientSecret = _configuration.GetValue<string>("AzureAD:ClientSecret");
            string resourceId = _configuration.GetValue<string>("AzureAD:ResourceId");
            ClientCredential credential = new ClientCredential(clientId, clientSecret);
            AuthenticationContext authContext = new AuthenticationContext($"https://login.microsoftonline.com/{tenant}");
            AuthenticationResult authenticationResult = null;
            string accessToken = string.Empty;

            try
            {
                authenticationResult = await authContext.AcquireTokenAsync(resourceId, credential);

            }
            catch (Exception ex)
            {
                // Log exception here
            }

            if (authenticationResult != null)
            {
                accessToken = authenticationResult.AccessToken;
            }

            return accessToken;
        }
    }
}
