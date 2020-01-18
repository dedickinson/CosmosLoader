using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CosmosQuery
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Utility tool for querying CosmosDB") {
                new Option (
                "--uri",
                "The CosmosDB URI") {
                Argument = new Argument<string> ()
                },
                new Option (
                "--key",
                "The CosmosDB Key") {
                Argument = new Argument<string> ()
                },
                new Option (
                "--db",
                "The CosmosDB Database") {
                Argument = new Argument<string> ()
                },
                new Option (
                "--container",
                "The CosmosDB container") {
                Argument = new Argument<string> ()
                },
                new Option (
                "--query",
                "The CosmosDB query") {
                Argument = new Argument<string> ()
                },
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string, string>((Uri, Key, Db, Container, Query) =>
            {
                CosmosClientOptions options = new CosmosClientOptions() { AllowBulkExecution = true };
                var Client = new CosmosClient(Uri, Key, options);
                var Database = Client.GetDatabase(Db);

                return RunQueryAsync(Query, Database.GetContainer(Container));

            });

            return rootCommand.InvokeAsync(args).Result;
        }
        static async Task<int> RunQueryAsync(string query, Container container)
        {
            Console.WriteLine($"Running query: {query}\n");
            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<dynamic> queryResultSetIterator = container.GetItemQueryIterator<dynamic>(query);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<dynamic> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var result in currentResultSet)
                {
                    Console.WriteLine(result);
                }
            }
            return 0;
        }
    }
}
