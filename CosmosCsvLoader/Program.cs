using System;

// See: https://github.com/dotnet/command-line-api/wiki/Features-overview
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text.Json;
using CsvHelper;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.Linq;

namespace CosmosCsvLoader
{
    class Program
    {

        static List<dynamic> ReadCSV(string CsvFilePath, string Delimiter = ",",
                        bool HasHeaderRecord = true)
        {
            List<dynamic> records;
            using (var reader = new StreamReader(CsvFilePath))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture))
            {
                csv.Configuration.HasHeaderRecord = HasHeaderRecord;
                csv.Configuration.Delimiter = Regex.Unescape(Delimiter);
                records = new List<dynamic>(csv.GetRecords<dynamic>());
            }
            return records;
        }

        static int Main(string[] args)
        {
            // Create a root command with some options
            // Example usage: dotnet run -- -h
            var rootCommand = new RootCommand("Utility tool for posting CSV data to CosmosDB")
            {
                new Option(
                    "--csv",
                    "The CSV file path")
                {
                    Argument = new Argument<FileInfo>()
                },
                new Option(
                    "--delimiter",
                    "The CSV file delimiter")
                {
                    Argument = new Argument<string>(defaultValue: () => ",")
                },
                new Option(
                    "--has-header",
                    "Indicates if the CSV has a header line")
                {
                    Argument = new Argument<bool>(defaultValue: () => true)
                },                
            };

            var JsonCommand = new Command("json")
            {
                Description = "Convert the CSV to JSON and output to stdout",
                Handler = CommandHandler.Create<FileInfo, string, bool>((Csv, Delimiter, HasHeader) =>
                {
                    if (!Csv.Exists)
                    {
                        Console.Error.WriteLine($"File not found: {Csv}");
                        return 1;
                    }
                    var records = ReadCSV(Csv.FullName, Delimiter, HasHeader);

                    if (records is null)
                    {
                        Console.Error.WriteLine("No records found");
                        return 1;
                    }
                    Console.Write(JsonSerializer.Serialize(records));
                    return 0;
                })
            };
            rootCommand.AddCommand(JsonCommand);
            

            var CosmosCommand = new Command("cosmos") 
            {
                new Option(
                    "--uri",
                    "The CosmosDB URI")
                {
                    Argument = new Argument<string>()
                },
                new Option(
                    "--key",
                    "The CosmosDB Key")
                {
                    Argument = new Argument<string>()
                },
                new Option(
                    "--db",
                    "The CosmosDB Database")
                {
                    Argument = new Argument<string>()
                },
                new Option(
                    "--container",
                    "The CosmosDB container")
                {
                    Argument = new Argument<string>()
                },
                new Option(
                    "--partition-path",
                    "The CosmosDB container partition (e.g. /State")
                {
                    Argument = new Argument<string>()
                },
            };
            
            CosmosCommand.Handler = CommandHandler.Create<FileInfo, string, bool, string, string, string, string>(async (Csv, Delimiter, HasHeader, Uri, Key, Db, Container) =>
            {
                var records = ReadCSV(Csv.FullName, Delimiter, HasHeader);

                // See: https://devblogs.microsoft.com/cosmosdb/introducing-bulk-support-in-the-net-sdk/
                CosmosClientOptions options = new CosmosClientOptions() { AllowBulkExecution = true };
                var Client = new CosmosClient(Uri, Key, options);
                Database CosmosDatabase = await Client.CreateDatabaseIfNotExistsAsync(Db);

                Container CosmosContainer = await CosmosDatabase.CreateContainerIfNotExistsAsync(Container, "/admin_code1");
                
                var TaskList = new List<Task>();
                Console.WriteLine($"Records to be uploaded: {records.Count}");
                foreach (var r in records)
                {
                    TaskList.Add(CosmosContainer.UpsertItemAsync(r));
                }
                Task.WaitAll(TaskList.ToArray());

                Console.WriteLine("Completed");
                Console.WriteLine($"Records uploaded: {TaskList.Count}");
            });

            rootCommand.AddCommand(CosmosCommand);


            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
