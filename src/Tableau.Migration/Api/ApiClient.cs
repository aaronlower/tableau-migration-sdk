﻿// Copyright (c) 2023, Salesforce, Inc.
//  SPDX-License-Identifier: Apache-2
//  
//  Licensed under the Apache License, Version 2.0 (the ""License"") 
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an ""AS IS"" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tableau.Migration.Api.Models;
using Tableau.Migration.Api.Rest.Models.Requests;
using Tableau.Migration.Api.Rest.Models.Responses;
using Tableau.Migration.Net.Rest;
using Tableau.Migration.Resources;

namespace Tableau.Migration.Api
{
    internal sealed class ApiClient : ApiClientBase, IApiClient
    {
        private readonly ISitesApiClient _sitesApiClient;
        private readonly TableauSiteConnectionConfiguration _siteConnectionConfiguration;
        private readonly IServerSessionProvider _sessionProvider;

        /// <summary>
        /// Creates a new <see cref="ApiClient"/> object.
        /// </summary>
        /// <param name="input">The API client input to initialize from.</param>
        /// <param name="restRequestBuilderFactory">The REST URI builder factory to initialize from.</param>
        /// <param name="tokenProvider">The authentication token provider to initialize from.</param>
        /// <param name="sessionProvider">The session provider to initialize from.</param>
        /// <param name="loggerFactory">The logger factory to initialize from.</param>
        /// <param name="sitesApiClient">The API client for site operations.</param>
        /// <param name="sharedResourcesLocalizer">A string localizer.</param>
        public ApiClient(
            IApiClientInput input,
            IRestRequestBuilderFactory restRequestBuilderFactory,
            IAuthenticationTokenProvider tokenProvider,
            IServerSessionProvider sessionProvider,
            ILoggerFactory loggerFactory,
            ISitesApiClient sitesApiClient,
            ISharedResourcesLocalizer sharedResourcesLocalizer)
            : base(restRequestBuilderFactory, loggerFactory, sharedResourcesLocalizer)
        {
            _siteConnectionConfiguration = input.SiteConnectionConfiguration;
            _sessionProvider = sessionProvider;
            _sitesApiClient = sitesApiClient;
            tokenProvider.RefreshRequestedAsync += async (cancel) =>
            {
                var signInResult = await GetSignInResultAsync(cancel).ConfigureAwait(false);

                if (signInResult.Success)
                {
                    tokenProvider.Set(signInResult.Value.Token);
                }
            };
        }

        public async Task<IAsyncDisposableResult<ISitesApiClient>> SignInAsync(CancellationToken cancel)
        {
            // Set the default API version if it hasn't been set already so we know which sign-in API version to call.
            if (_sessionProvider.Version is null)
            {
                var serverInfoResult = await GetServerInfoAsync(cancel).ConfigureAwait(false);

                if (!serverInfoResult.Success)
                    return AsyncDisposableResult<ISitesApiClient>.Failed(serverInfoResult.Errors);
            }

            var signInResult = await GetSignInResultAsync(cancel).ConfigureAwait(false);

            if (!signInResult.Success)
            {
                return AsyncDisposableResult<ISitesApiClient>.Failed(signInResult.Errors);
            }

            _sessionProvider.SetCurrentUserAndSite(signInResult.Value);

            return AsyncDisposableResult<ISitesApiClient>.Succeeded(_sitesApiClient);
        }

        private async Task<IResult<ISignInResult>> GetSignInResultAsync(CancellationToken cancel)
        {
            var signInResult = await RestRequestBuilderFactory
                .CreateUri("/auth/signin")
                .WithSiteId(null)
                .ForPostRequest()
                .WithXmlContent(new SignInRequest(_siteConnectionConfiguration))
                .SendAsync<SignInResponse>(cancel)
                .ToResultAsync<SignInResponse, ISignInResult>(r => new SignInResult(r), SharedResourcesLocalizer)
                .ConfigureAwait(false);

            return signInResult;
        }

        public async Task<IResult<IServerInfo>> GetServerInfoAsync(CancellationToken cancel)
        {
            // The first version this endpoint is available.
            // This is needed because we won't know the actual server version prior to this call.
            const string MINIMUM_API_VERSION = "2.4"; // Tableau Server 9.3

            var serverInfoResult = await RestRequestBuilderFactory
                .CreateUri("/serverinfo")
                .WithApiVersion(_sessionProvider.Version?.RestApiVersion ?? MINIMUM_API_VERSION)
                .WithSiteId(null)
                .ForGetRequest()
                .SendAsync<ServerInfoResponse>(cancel)
                .ToResultAsync<ServerInfoResponse, IServerInfo>(r => new ServerInfo(r), SharedResourcesLocalizer)
                .ConfigureAwait(false);

            if (serverInfoResult.Success)
                _sessionProvider.SetVersion(serverInfoResult.Value.TableauServerVersion);

            return serverInfoResult;
        }
    }
}
