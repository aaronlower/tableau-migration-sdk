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

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tableau.Migration.Api;
using Tableau.Migration.Net.Rest;

namespace Tableau.Migration.Net.Handlers
{
    internal class AuthenticationHandler : DelegatingHandler
    {
        private readonly IAuthenticationTokenProvider _tokenProvider;

        public AuthenticationHandler(IAuthenticationTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.IsRest() && !request.RequestUri.IsRestSignIn())
            {
                // Use the current token
                if (_tokenProvider.Token is not null)
                    request.SetRestAuthenticationToken(_tokenProvider.Token);

                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await _tokenProvider.RequestRefreshAsync(cancellationToken).ConfigureAwait(false);

                    // Use the new token
                    if (_tokenProvider.Token is not null)
                        request.SetRestAuthenticationToken(_tokenProvider.Token);
                }
                else
                {
                    return response;
                }
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
