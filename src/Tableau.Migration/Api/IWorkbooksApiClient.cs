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

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Tableau.Migration.Api.Models;
using Tableau.Migration.Api.Permissions;
using Tableau.Migration.Api.Tags;
using Tableau.Migration.Content;
using Tableau.Migration.Content.Files;
using Tableau.Migration.Paging;

namespace Tableau.Migration.Api
{
    /// <summary>
    /// Interface for API client workbook operations.
    /// </summary>
    public interface IWorkbooksApiClient :
        IPagedListApiClient<IWorkbook>,
        IPublishApiClient<IPublishableWorkbook, IResultWorkbook>,
        IPullApiClient<IWorkbook, IPublishableWorkbook>,
        IOwnershipApiClient,
        ITagsContentApiClient,
        IApiPageAccessor<IWorkbook>,
        IPermissionsContentApiClient,
        IConnectionsApiClient
    {
        /// <summary>
        /// Gets all workbook in the current site.
        /// </summary>
        /// <param name="pageNumber">The 1-indexed page number.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="cancel">A cancellation token to obey.</param>
        /// <returns>A list of a page of workbooks in the current site.</returns>
        Task<IPagedResult<IWorkbook>> GetAllWorkbooksAsync(int pageNumber, int pageSize, CancellationToken cancel);

        /// <summary>
        /// Gets a workbook by the given ID.
        /// </summary>
        /// <param name="workbookId">The ID to get the workbook for.</param>
        /// <param name="connections">The workbook connection metadata.</param>
        /// <param name="workbookFile">The workbook file.</param>
        /// <param name="cancel">A cancellation token to obey.</param>
        /// <returns>The data sorce result.</returns>
        Task<IResult<IPublishableWorkbook>> GetWorkbookAsync(Guid workbookId,
            IImmutableList<IConnection> connections,
            IContentFileHandle workbookFile, CancellationToken cancel);

        /// <summary>
        /// Downloads the workbook file for the given ID.
        /// </summary>
        /// <param name="workbookId">The ID to download the workbook file for.</param>
        /// <param name="includeExtract">Whether or not to include extracts in the workbook file.</param>
        /// <param name="cancel">A cancellation token to obey.</param>
        /// <returns>The file download result.</returns>
        Task<IAsyncDisposableResult<FileDownload>> DownloadWorkbookAsync(
            Guid workbookId,
            bool includeExtract,
            CancellationToken cancel);

        /// <summary>
        /// Uploads the input workbook file.
        /// </summary>
        /// <param name="options">The new workbook's details.</param>
        /// <param name="cancel">A cancellation token to obey.</param>
        /// <returns>The published workbook.</returns>
        Task<IResult<IResultWorkbook>> PublishWorkbookAsync(
            IPublishWorkbookOptions options,
            CancellationToken cancel);

        /// <summary>
        /// Updates the workbook after publishing.
        /// </summary>
        /// <param name="workbookId">The ID for the workbook to update.</param>
        /// <param name="cancel">A cancellation token to obey.</param>
        /// <param name="newName">The new name of the workbook, or null to not update the name.</param>
        /// <param name="newDescription">The new description of the workbook, or null to not update the description.</param>
        /// <param name="newProjectId">The LUID of a project to move the  workbook to, or null to not update the project.</param>
        /// <param name="newOwnerId">The LUID of a user to assign the workbook to as owner, or null to not update the owner.</param>
        /// <param name="newShowTabs">Whether or not to show workbook views in tabs, or null to not update the option.</param>
        /// <param name="newRecentlyViewed">Whether or not to show the workbook in the recently viewed list, or null to not update the flag.</param>
        /// <param name="newEncryptExtracts">Whether or not to encrypt extracts, or null to not update the option.</param>
        /// <returns>The update result.</returns>
        Task<IResult<IUpdateWorkbookResult>> UpdateWorkbookAsync(
            Guid workbookId,
            CancellationToken cancel,
            string? newName = null,
            string? newDescription = null,
            Guid? newProjectId = null,
            Guid? newOwnerId = null,
            bool? newShowTabs = null,
            bool? newRecentlyViewed = null,
            bool? newEncryptExtracts = null);
    }
}
