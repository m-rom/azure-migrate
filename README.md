# Migrate/copy/restruct data stored in Azure

## How to?

1. Clone the project
2. Go to `Program.cs` 
3. Change `SourceModel` to fit your needs
4. Change `TargetModel` to fit your needs
5. [optional] Add `AutoMapper` configurations at the top of the file if needed
6. Add the connection settings directly to the file (like it was done in the comments)
7. Run it! (No warrenty. It's a simple mostly helpful tool done in 2 hours) :blush:

## Supported

Source | Target
-|-
Azure Storage Table | Azure Storage Table
Azure Storage Table | Azure CosmosDB Collection
Azure CosmosDB Collection | Azure Storage Table
Azure CosmosDB Collection | Azure CosmosDB Collection
