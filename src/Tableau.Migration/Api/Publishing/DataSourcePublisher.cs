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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tableau.Migration.Api.Models;
using Tableau.Migration.Api.Rest;
using Tableau.Migration.Api.Rest.Models.Requests;
using Tableau.Migration.Api.Rest.Models.Responses;
using Tableau.Migration.Content;
using Tableau.Migration.Content.Search;
using Tableau.Migration.Net;
using Tableau.Migration.Net.Rest;
using Tableau.Migration.Resources;

namespace Tableau.Migration.Api.Publishing
{
    internal class DataSourcePublisher : FilePublisherBase<IPublishDataSourceOptions, CommitDataSourcePublishRequest, IDataSource>, IDataSourcePublisher
    {
        public DataSourcePublisher(
            IRestRequestBuilderFactory restRequestBuilderFactory,
            IContentReferenceFinderFactory finderFactory,
            IServerSessionProvider sessionProvider,
            ISharedResourcesLocalizer sharedResourcesLocalizer,
            IHttpStreamProcessor httpStreamProcessor)
            : base(
                  restRequestBuilderFactory,
                  finderFactory,
                  sessionProvider,
                  sharedResourcesLocalizer,
                  httpStreamProcessor,
                  RestUrlPrefixes.DataSources)
        { }

        protected override CommitDataSourcePublishRequest BuildCommitRequest(IPublishDataSourceOptions options)
            => new(options);

        protected override async Task<IResult<IDataSource>> SendCommitRequestAsync(
            IPublishDataSourceOptions options,
            string uploadSessionId,
            MultipartContent content,
            CancellationToken cancel)
        {
            var request = RestRequestBuilderFactory
               .CreateUri(ContentTypeUrlPrefix)
               .WithQuery("uploadSessionId", uploadSessionId)
               .WithQuery("datasourceType", options.FileType)
               .WithQuery("overwrite", options.Overwrite.ToString().ToLower())
               .ForPostRequest()
               .WithContent(content);

            var result = await request
                .SendAsync<DataSourceResponse>(cancel)
                    .ToResultAsync<DataSourceResponse, IDataSource>(async (r, c) =>
                    {
                        var project = await ContentFinderFactory.FindProjectAsync(r.Item, c).ConfigureAwait(false);
                        var owner = await ContentFinderFactory.FindOwnerAsync(r.Item, c).ConfigureAwait(false);
                        return new DataSource(r.Item, project, owner);
                    },
                    SharedResourcesLocalizer,
                    cancel)
                .ConfigureAwait(false);

            return result;
        }
    }
}
