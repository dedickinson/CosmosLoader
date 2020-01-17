# Cosmos CSV Loader

```powsershell
dotnet run -- --csv C:\Development\CosmosPostcodes\data\AU.csv json 
```

```powershell
# Connect-AzAccount

$resourceGroup = "az-203-training"
$cosmosName = "az-203-cosmos"
$cosmosDb = "geonames"
$cosmosContainer = "postcodes"
$cosmosUri = "https://$cosmosName.documents.azure.com:443/"

$cosmosKey = (Invoke-AzResourceAction -Action listKeys `
    -ResourceType "Microsoft.DocumentDb/databaseAccounts" `
    -ApiVersion "2015-04-08" `
    -ResourceGroupName $resourceGroup `
    -Name $cosmosName).primaryMasterKey

dotnet run -- --csv C:\Development\CosmosPostcodes\data\AU.csv cosmos --uri $CosmosUri --key $cosmosKey --db $cosmosDb --container $cosmosContainer
```

## SQL API Queries

Record count:

    SELECT value count(1) FROM c