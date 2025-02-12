# Configuration

The Migration SDK uses two sources of configuration in two blocks: the [Migration Plan](#migration-plan) that contains configuration for a specific migration run, and [Migration SDK Options](#migration-sdk-options) for configuration that is unlikely to change between migration runs.

![Configuration Blocks](../images/configuration.png)

## Migration Plan
The migration plan is a required input in the migration process. It will define the Source and Destination servers and the hooks executed during the migration. Consider the Migration Plan as the steps the Migration SDK will follow to migrate the information from a given Source Server to a given Destination Server.

The [`IMigrationPlan`](xref:Tableau.Migration.IMigrationPlan) interface defines the Migration Plan structure. And the easiest path to generate a new Migration Plan is the [`IMigrationPlanBuilder`](xref:Tableau.Migration.IMigrationPlanBuilder) implementation [**`MigrationPlanBuilder`**](xref:Tableau.Migration.Engine.MigrationPlanBuilder). For that, it is needed a few steps before [building](#build) a new plan:
- [Define the required Source server](#source).
- [Define the required Destination server](#destination).
- [Define the required Migration Type](#migration-type).
- [Add supplementary hooks](#add-hooks).

### Source
*Optional/Required:* **Required**.

*Description:* The method [`MigrationPlanBuilder.FromSourceTableauServer`](xref:Tableau.Migration.Engine.MigrationPlanBuilder#Tableau_Migration_Engine_MigrationPlanBuilder_FromSourceTableauServer_System_Uri_System_String_System_String_System_String_System_Boolean_) will define the source server by instantiating a new [`TableauSiteConnectionConfiguration`](xref:Tableau.Migration.Api.TableauSiteConnectionConfiguration) with the following parameters:
- **serverUrl:** Required.
- **siteContentUrl:** Optional.
- **accessTokenName:** Required.
- **accessToken:** Required.
> [!Important]
> Personal access tokens (PATs) are long-lived authentication tokens that allow you to sign in to the Tableau REST API without requiring hard-coded credentials or interactive signin. Revoke and generate a new PAT every day to keep your server secure. Access tokens should not be stored in plain text in application configuration files, and should instead use secure alternatives such as encryption or a secrets management system. If the source and destination sites are on the same server, separate PATs should be used.

### Destination
*Optional/Required:* **Required**.

*Description:* The method [`MigrationPlanBuilder.ToDestinationTableauCloud`](xref:Tableau.Migration.Engine.MigrationPlanBuilder#Tableau_Migration_Engine_MigrationPlanBuilder_ToDestinationTableauCloud_System_Uri_System_String_System_String_System_String_System_Boolean_) will define the destination server by instantiating a new [`TableauSiteConnectionConfiguration`](xref:Tableau.Migration.Api.TableauSiteConnectionConfiguration) with the following parameters:
- **podUrl:** Required.
- **siteContentUrl:** Required. This is the site name on Tableau Cloud.
- **accessTokenName:** Required.
- **accessToken:** Required.
> [!Important]
> Personal access tokens (PATs) are long-lived authentication tokens that allow you to sign in to the Tableau REST API without requiring hard-coded credentials or interactive signin. Revoke and generate a new PAT every day to keep your server secure. Access tokens should not be stored in plain text in application configuration files, and should instead use secure alternatives such as encryption or a secrets management system. If the source and destination sites are on the same server, separate PATs should be used.

### Migration Type
*Optional/Required:* **Required**.

*Description:* The method [`MigrationPlanBuilder.ForServerToCloud`](xref:Tableau.Migration.Engine.MigrationPlanBuilder#Tableau_Migration_Engine_MigrationPlanBuilder_ForServerToCloud) will define the migration type and load all default hooks for a **Server to Cloud** migration.

### Add Hooks
*Optional/Required:* **Optional**.

*Description:* The Plan Builder exposes the properties [`MigrationPlanBuilder.Hooks`](xref:Tableau.Migration.Engine.MigrationPlanBuilder#Tableau_Migration_Engine_MigrationPlanBuilder_Hooks), [`MigrationPlanBuilder.Filters`](xref:Tableau.Migration.Engine.MigrationPlanBuilder#Tableau_Migration_Engine_MigrationPlanBuilder_Filters), [`MigrationPlanBuilder.Mappings`](xref:Tableau.Migration.Engine.MigrationPlanBuilder#Tableau_Migration_Engine_MigrationPlanBuilder_Mappings), and [`MigrationPlanBuilder.Transformers`](xref:Tableau.Migration.Engine.MigrationPlanBuilder#Tableau_Migration_Engine_MigrationPlanBuilder_Transformers). With these properties, it is possible to adjust a given migration plan for specific scenarios. For more details, check the [Custom Hooks article](advanced_config/hooks/custom_hooks.md).

### Build
*Optional/Required:* **Required**.

*Description:* The method [`MigrationPlanBuilder.Build`](xref:Tableau.Migration.Engine.MigrationPlanBuilder#Tableau_Migration_Engine_MigrationPlanBuilder_Build) will generate a Migration Plan ready to be used as an input to a migration process.

## Migration SDK Options

[`MigrationSdkOptions`](xref:Tableau.Migration.Config.MigrationSdkOptions) is the configuration class the Migration SDK uses internally to process a migration. It contains adjustable properties that change some engine behaviors. These properties are useful tools to troubleshoot and tune a migration process. Start with this class and others in the [Config](xref:Tableau.Migration.Config) section for more details.


### BatchSize
*Reference:* [`MigrationSdkOptions.BatchSize`](xref:Tableau.Migration.Config.MigrationSdkOptions#Tableau_Migration_Config_MigrationSdkOptions_BatchSize).

*Default:* [`MigrationSdkOptions.Defaults.BATCH_SIZE`](xref:Tableau.Migration.Config.MigrationSdkOptions.Defaults#Tableau_Migration_Config_MigrationSdkOptions_Defaults_BATCH_SIZE).

*Reload on Edit?:* **Yes**. The update will apply next time the Migration SDK requests a list of objects.

*Description:* The Migration SDK uses the **BatchSize** property to define the page size of each List Request. For more details, check the [Tableau REST API Paginating Results documentation](https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_concepts_paging.htm).


### MigrationParallelism
*Reference:* [`MigrationSdkOptions.MigrationParallelism`](xref:Tableau.Migration.Config.MigrationSdkOptions#Tableau_Migration_Config_MigrationSdkOptions_MigrationParallelism).

*Default:* [`MigrationSdkOptions.Defaults.MIGRATION_PARALLELISM`](xref:Tableau.Migration.Config.MigrationSdkOptions.Defaults#Tableau_Migration_Config_MigrationSdkOptions_Defaults_MIGRATION_PARALLELISM).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK publishes a new batch.

*Description:* The Migration SDK uses [two methods](advanced_config/hooks/index.md#hook-execution-flow) to publish the content to a destination server: the **bulk process**, where a single call to the API will push multiple items to the server, and the **individual process**, where it publishes a single item with a single call to the API. This configuration only applies to the **individual process**. The SDK uses the **MigrationParallelism** property to define the number of parallel tasks migrating the same type of content simultaneosly. It is possible to tune the Migration SDK processing time with this configuration.
> [!WARNING]
> There are [concurrency limits in REST APIs on Tableau Cloud](https://kb.tableau.com/articles/issue/concurrency-limits-in-rest-apis-on-tableau-cloud). The current default configuration is the balance between performance without blocking too many resources to the migration process.


### Files.DisableFileEncryption
*Reference:* [`FileOptions.DisableFileEncryption`](xref:Tableau.Migration.Config.FileOptions#Tableau_Migration_Config_FileOptions_DisableFileEncryption).

*Default:* [`FileOptions.Defaults.DISABLE_FILE_ENCRYPTION`](xref:Tableau.Migration.Config.FileOptions.Defaults#Tableau_Migration_Config_FileOptions_Defaults_DISABLE_FILE_ENCRYPTION).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK executes a migration plan.

*Description:* As part of the migration process, the Migration SDK has to adjust existing references for file-based content types like Workbooks and Data Sources. The SDK has to download and temporarily store the content in the migration machine to be able to read and edit these files. The Migration SDK uses the **DisableFileEncryption** property to define whether it will encrypt the temporary file.
> [!CAUTION]
> Do not disable file encryption when migrating production content.


### Files.RootPath
*Reference:* [`FileOptions.RootPath`](xref:Tableau.Migration.Config.FileOptions#Tableau_Migration_Config_FileOptions_RootPath).

*Default:* [`FileOptions.Defaults.ROOT_PATH`](xref:Tableau.Migration.Config.FileOptions.Defaults#Tableau_Migration_Config_FileOptions_Defaults_ROOT_PATH).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK executes a migration plan.

*Description:* As part of the migration process, the Migration SDK has to adjust existing references for file-based content types like Workbooks and Data Sources. The SDK has to download and temporarily store the content in the migration machine to be able to read and edit these files. The Migration SDK uses the **RootPath** property to define the location where it will store the temporary files.


### Network.FileChunkSizeKB
*Reference:* [`NetworkOptions.FileChunkSizeKB`](xref:Tableau.Migration.Config.NetworkOptions#Tableau_Migration_Config_NetworkOptions_FileChunkSizeKB).

*Default:* [`NetworkOptions.Defaults.FILE_CHUNK_SIZE_KB`](xref:Tableau.Migration.Config.NetworkOptions.Defaults#Tableau_Migration_Config_NetworkOptions_Defaults_FILE_CHUNK_SIZE_KB).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK publishes a new file.

*Description:* As part of the migration process, the Migration SDK has to publish file-based content types like Workbooks and Data Sources. Some of these files are very large. The Migration SDK uses the **FileChunkSizeKB** property to split these files into smaller pieces, making the publishing process more reliable. For more details, check the [Tableau REST API Publishing Resources documentation](https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_concepts_publish.htm).


### Network.HeadersLoggingEnabled
*Reference:* [`NetworkOptions.HeadersLoggingEnabled`](xref:Tableau.Migration.Config.NetworkOptions#Tableau_Migration_Config_NetworkOptions_HeadersLoggingEnabled).

*Default:* [`NetworkOptions.Defaults.LOG_HEADERS_ENABLED`](xref:Tableau.Migration.Config.NetworkOptions.Defaults#Tableau_Migration_Config_NetworkOptions_Defaults_LOG_HEADERS_ENABLED).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK logs a new HTTP request.

*Description:* Check the [logging article](logging.md) for more details.


### Network.ContentLoggingEnabled
*Reference:* [`NetworkOptions.ContentLoggingEnabled`](xref:Tableau.Migration.Config.NetworkOptions#Tableau_Migration_Config_NetworkOptions_ContentLoggingEnabled).

*Default:* [`NetworkOptions.Defaults.LOG_CONTENT_ENABLED`](xref:Tableau.Migration.Config.NetworkOptions.Defaults#Tableau_Migration_Config_NetworkOptions_Defaults_LOG_CONTENT_ENABLED).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK logs a new HTTP request.

*Description:* Check the [logging article](logging.md) for more details.


### Network.BinaryContentLoggingEnabled
*Reference:* [`NetworkOptions.BinaryContentLoggingEnabled`](xref:Tableau.Migration.Config.NetworkOptions#Tableau_Migration_Config_NetworkOptions_BinaryContentLoggingEnabled).

*Default:* [`NetworkOptions.Defaults.LOG_BINARY_CONTENT_ENABLED`](xref:Tableau.Migration.Config.NetworkOptions.Defaults#Tableau_Migration_Config_NetworkOptions_Defaults_LOG_BINARY_CONTENT_ENABLED).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK logs a new HTTP request.

*Description:* Check the [logging article](logging.md) for more details.


### Network.ExceptionsLoggingEnabled
*Reference:* [`NetworkOptions.ExceptionsLoggingEnabled`](xref:Tableau.Migration.Config.NetworkOptions#Tableau_Migration_Config_NetworkOptions_ExceptionsLoggingEnabled).

*Default:* [`NetworkOptions.Defaults.LOG_EXCEPTIONS_ENABLED`](xref:Tableau.Migration.Config.NetworkOptions.Defaults#Tableau_Migration_Config_NetworkOptions_Defaults_LOG_EXCEPTIONS_ENABLED).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK logs a new HTTP request.

*Description:* Check the [logging article](logging.md) for more details.


### Network.Resilience.RetryEnabled
*Reference:* [`ResilienceOptions.RetryEnabled`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_RetryEnabled).

*Default:* [`ResilienceOptions.Defaults.RETRY_ENABLED`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_RETRY_ENABLED).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **RetryEnabled** property to define whether it will retry failed requests.


### Network.Resilience.RetryIntervals
*Reference:* [`ResilienceOptions.RetryIntervals`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_RetryIntervals).

*Default:* [`ResilienceOptions.Defaults.RETRY_INTERVALS`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_RETRY_INTERVALS).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **RetryIntervals** property to define the number of retries and the interval between each retry.


### Network.Resilience.RetryOverrideResponseCodes
*Reference:* [`ResilienceOptions.RetryOverrideResponseCodes`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_RetryOverrideResponseCodes).

*Default:* [`ResilienceOptions.Defaults.RETRY_OVERRIDE_RESPONSE_CODES`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_RETRY_OVERRIDE_RESPONSE_CODES).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **RetryOverrideResponseCodes** property to override the default list of error status codes for retries with a specific list of status codes.


### Network.Resilience.ConcurrentRequestsLimitEnabled
*Reference:* [`ResilienceOptions.ConcurrentRequestsLimitEnabled`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_ConcurrentRequestsLimitEnabled).

*Default:* [`ResilienceOptions.Defaults.CONCURRENT_REQUESTS_LIMIT_ENABLED`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_CONCURRENT_REQUESTS_LIMIT_ENABLED).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **ConcurrentRequestsLimitEnabled** property to define whether it will limit concurrent requests.


### Network.Resilience.MaxConcurrentRequests
*Reference:* [`ResilienceOptions.MaxConcurrentRequests`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_MaxConcurrentRequests).

*Default:* [`ResilienceOptions.Defaults.MAX_CONCURRENT_REQUESTS`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_MAX_CONCURRENT_REQUESTS).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **MaxConcurrentRequests** property to define the maximum quantity of concurrent API requests.


### Network.Resilience.ConcurrentWaitingRequestsOnQueue
*Reference:* [`ResilienceOptions.ConcurrentWaitingRequestsOnQueue`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_ConcurrentWaitingRequestsOnQueue).

*Default:* [`ResilienceOptions.Defaults.CONCURRENT_WAITING_REQUESTS_QUEUE`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_CONCURRENT_WAITING_REQUESTS_QUEUE).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **ConcurrentWaitingRequestsOnQueue** property to define the quantity of concurrent API requests waiting on queue.


### Network.Resilience.ClientThrottleEnabled
*Reference:* [`ResilienceOptions.ClientThrottleEnabled`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_ClientThrottleEnabled).

*Default:* [`ResilienceOptions.Defaults.CLIENT_THROTTLE_ENABLED`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_CLIENT_THROTTLE_ENABLED).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **ClientThrottleEnabled** property to define whether it will limit requests to a given endpoint on the client side.


### Network.Resilience.MaxReadRequests
*Reference:* [`ResilienceOptions.MaxReadRequests`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_MaxReadRequests).

*Default:* [`ResilienceOptions.Defaults.MAX_READ_REQUESTS`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_MAX_READ_REQUESTS).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **MaxReadRequests** property to define the maximum quantity of GET requests on the client side.


### Network.Resilience.MaxReadRequestsInterval
*Reference:* [`ResilienceOptions.MaxReadRequestsInterval`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_MaxReadRequestsInterval).

*Default:* [`ResilienceOptions.Defaults.MAX_READ_REQUESTS_INTERVAL`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_MAX_READ_REQUESTS_INTERVAL).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **MaxReadRequestsInterval** property to define the interval for the limit of GET requests on the client side.


### Network.Resilience.MaxBurstReadRequests
*Reference:* [`ResilienceOptions.MaxBurstReadRequests`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_MaxBurstReadRequests).

*Default:* [`ResilienceOptions.Defaults.MAX_BURST_READ_REQUESTS`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_MAX_BURST_READ_REQUESTS).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. With only the previous configuration values (`Network.Resilience.MaxReadRequests` and `Network.Resilience.MaxReadRequestsInterval`), the SDK will calculate the minimum interval to complete a single request. Any other request at the same period will be blocked. The SDK uses the **MaxBurstReadRequests** property to define the maximum quantity of GET requests on the calculated period.


### Network.Resilience.MaxPublishRequests
*Reference:* [`ResilienceOptions.MaxPublishRequests`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_MaxPublishRequests).

*Default:* [`ResilienceOptions.Defaults.MAX_PUBLISH_REQUESTS`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_MAX_PUBLISH_REQUESTS).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **MaxPublishRequests** property to define the maximum quantity of non-GET requests on the client side.


### Network.Resilience.MaxPublishRequestsInterval
*Reference:* [`ResilienceOptions.MaxPublishRequestsInterval`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_MaxPublishRequestsInterval).

*Default:* [`ResilienceOptions.Defaults.MAX_PUBLISH_REQUESTS_INTERVAL`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_MAX_PUBLISH_REQUESTS_INTERVAL).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **MaxPublishRequestsInterval** property to define the interval for the limit of non-GET requests on the client side.


### Network.Resilience.MaxBurstPublishRequests
*Reference:* [`ResilienceOptions.MaxBurstPublishRequests`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_MaxBurstPublishRequests).

*Default:* [`ResilienceOptions.Defaults.MAX_BURST_PUBLISH_REQUESTS`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_MAX_BURST_PUBLISH_REQUESTS).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. With only the previous configuration values (`Network.Resilience.MaxPublishRequests` and `Network.Resilience.MaxPublishRequestsInterval`), the SDK will calculate the minimum interval to complete a single request. Any other request at the same period will be blocked. The SDK uses the **MaxBurstPublishRequests** property to define the maximum quantity of non-GET requests on the calculated period.


### Network.Resilience.ServerThrottleEnabled
*Reference:* [`ResilienceOptions.ServerThrottleEnabled`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_ServerThrottleEnabled).

*Default:* [`ResilienceOptions.Defaults.SERVER_THROTTLE_ENABLED`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_SERVER_THROTTLE_ENABLED).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **ServerThrottleEnabled** property to define whether it will retry requests throttled on the server.


### Network.Resilience.ServerThrottleLimitRetries
*Reference:* [`ResilienceOptions.ServerThrottleLimitRetries`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_ServerThrottleLimitRetries).

*Default:* [`ResilienceOptions.Defaults.SERVER_THROTTLE_LIMIT_RETRIES`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_SERVER_THROTTLE_LIMIT_RETRIES).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **ServerThrottleLimitRetries** property to define whether it will have a limit of retries to a throttled request.


### Network.Resilience.ServerThrottleRetryIntervals
*Reference:* [`ResilienceOptions.ServerThrottleRetryIntervals`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_ServerThrottleRetryIntervals).

*Default:* [`ResilienceOptions.Defaults.SERVER_THROTTLE_RETRY_INTERVALS`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_SERVER_THROTTLE_RETRY_INTERVALS).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **ServerThrottleRetryIntervals** property to define the interval between each retry for throttled requests without the 'Retry-After' header. If `ServerThrottleLimitRetries` is enabled, this configuration defines the maximum number of retries. Otherwise, the subsequent retries use the last interval value.


### Network.Resilience.PerRequestTimeout
*Reference:* [`ResilienceOptions.PerRequestTimeout`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_PerRequestTimeout).

*Default:* [`ResilienceOptions.Defaults.REQUEST_TIMEOUT`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_REQUEST_TIMEOUT).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **PerRequestTimeout** property to define the maximum duration of non-FileTransfer requests.


### Network.Resilience.PerFileTransferRequestTimeout
*Reference:* [`ResilienceOptions.PerFileTransferRequestTimeout`](xref:Tableau.Migration.Config.ResilienceOptions#Tableau_Migration_Config_ResilienceOptions_PerFileTransferRequestTimeout).

*Default:* [`ResilienceOptions.Defaults.FILE_TRANSFER_REQUEST_TIMEOUT`](xref:Tableau.Migration.Config.ResilienceOptions.Defaults#Tableau_Migration_Config_ResilienceOptions_Defaults_FILE_TRANSFER_REQUEST_TIMEOUT).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK makes a new HTTP request.

*Description:* The Migration SDK uses [Polly](https://github.com/App-vNext/Polly) as a resilience and transient-fault layer. The SDK uses the **PerFileTransferRequestTimeout** property to define the maximum duration of FileTransfer requests.


### DefaultPermissionsContentTypes.UrlSegments
*Reference:* [`DefaultPermissionsContentTypeOptions.UrlSegments`](xref:Tableau.Migration.Config.DefaultPermissionsContentTypeOptions#Tableau_Migration_Config_DefaultPermissionsContentTypeOptions_UrlSegments).

*Default:* [`DefaultPermissionsContentTypeUrlSegments`](xref:Tableau.Migration.Content.Permissions.DefaultPermissionsContentTypeUrlSegments).

*Reload on Edit?:* **No**. Any changes to this configuration will reflect on the next time the application starts.

*Description:* The SDK uses the **UrlSegments** property as a list of types of default permissions of given project. For more details, check the [Query Default Permissions documentation](https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref_permissions.htm#query_default_permissions).


### Jobs.JobPollRate
*Reference:* [`JobOptions.JobPollRate`](xref:Tableau.Migration.Config.JobOptions#Tableau_Migration_Config_JobOptions_JobPollRate).

*Default:* [`JobOptions.Defaults.JOB_POLL_RATE`](xref:Tableau.Migration.Config.JobOptions.Defaults#Tableau_Migration_Config_JobOptions_Defaults_JOB_POLL_RATE).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK delays the processing status recheck.

*Description:* The Migration SDK uses [two methods](advanced_config/hooks/index.md#hook-execution-flow) to publish the content to a destination server: the **bulk process**, where a single call to the API will push multiple items to the server, and the **individual process**, where it publishes a single item with a single call to the API. This configuration only applies to the **bulk process**. After publishing a batch, the API will return a Job ID. With it, the SDK can call another API to check the job processing status. The SDK uses the **JobPollRate** property to define the interval it will wait to recheck processing status. For more details, check the [Tableau REST API Query Job documentation](https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref_jobs_tasks_and_schedules.htm#query_job).
> [!WARNING]
> There is [a limit for querying job status on Tableau Cloud](https://help.tableau.com/current/online/en-us/to_site_capacity.htm#jobs-initiated-by-command-line-and-api-calls). The current default configuration is the balance between performance without blocking too many resources to the migration process.


### Jobs.JobTimeout
*Reference:* [`JobOptions.JobTimeout`](xref:Tableau.Migration.Config.JobOptions#Tableau_Migration_Config_JobOptions_JobTimeout).

*Default:* [`JobOptions.Defaults.JOB_TIMEOUT`](xref:Tableau.Migration.Config.JobOptions.Defaults#Tableau_Migration_Config_JobOptions_Defaults_JOB_TIMEOUT).

*Reload on Edit?:* **Yes**. The update will apply the next time the Migration SDK validates the total time it has waited for the job to complete.

*Description:* The Migration SDK uses [two methods](advanced_config/hooks/index.md#hook-execution-flow) to publish the content to a destination server: the **bulk process**, where a single call to the API will push multiple items to the server, and the **individual process**, where it publishes a single item with a single call to the API. This configuration only applies to the **bulk process**. The SDK uses the **JobTimeout** property to define the maximum interval it will wait for a job to complete. For more details, check the [Tableau REST API Query Job documentation](https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref_jobs_tasks_and_schedules.htm#query_job).
