# SecureFileUploadFunction

An Azure Function for managing files in Azure Blob Storage.

Stack:
- Azure Functions
- .NET6
- CosmosDB
- Azure Blob Storage

This is the *v1* function - has standalone auth handled in isolation by the function via a table of users in CosmosDB. Offers registration and login capabilities.
Users can then manage their own files in Azure Blob Storage.

The *master* and *v2* branches authorize users via Zitadel.

## Dependencies:

- The [`DZarsky.CommonLibraries.AzureFunctions`](https://nuget.dzarsky.eu/packages/dzarsky.commonlibraries.azurefunctions/1.1.1) library (version <1.2.0)
- Azure account
- CosmosDB instance
- Azure Blob Storage
