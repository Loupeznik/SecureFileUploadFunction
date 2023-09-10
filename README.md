# SecureFileUploadFunction

An Azure Function for managing files in Azure Blob Storage.

Stack:
- Azure Functions
- .NET6
- CosmosDB
- Azure Blob Storage
- Zitadel

Authentication is achieved via Zitadel using OIDC and token introspection on the function side.

## Dependencies:

- The [`DZarsky.CommonLibraries.AzureFunctions`](https://nuget.dzarsky.eu/packages/dzarsky.commonlibraries.azurefunctions/1.2.1) library (version >1.2.1, preferably latest)
- Azure account
- CosmosDB instance
- Azure Blob Storage
- Zitadel instance (either self-hosted or cloud solution)
    - Project, application and credentials created in Zitadel admin console (application type should be API)
