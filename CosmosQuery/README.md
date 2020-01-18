
```bash
resourceGroup="az-203-training"
cosmosName="az-203-cosmos"
cosmosDb="geonames"
cosmosContainer="postcodes"
cosmosUri="https://$cosmosName.documents.azure.com:443/"

cosmosKey=$(az cosmosdb keys list --resource-group $resourceGroup \
    --name $cosmosName \
    --query primaryMasterKey -o tsv)

dotnet run -- --uri $cosmosUri --key $cosmosKey --db $cosmosDb --container $cosmosContainer --query "select TOP 5 * from c"

dotnet run -- --uri $cosmosUri --key $cosmosKey --db $cosmosDb --container $cosmosContainer \
    --query "select c.postal_code from c where c.place_name='Smithfield'"
```
