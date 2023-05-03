using Azure.Data.Tables;
namespace ConsoleAppDotNet6
{
    public record DataItem(string Id, string Name);
    internal static class AzStorage
    {
        public const string PurecloudUsers = "PurecloudUsers";
        public const string PurecloudQueues = "PurecloudQueues";
        public static async Task<List<DataItem>> LoadDataAsync(PurecloudAppConfig config, string table)
        {
            var tableClient = new TableServiceClient(config.AzStorage);
            var users = tableClient.GetTableClient(table);
            var res = new List<TableEntity>();
            await foreach (var item in users.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{config.OrgId}'", maxPerPage: 100))
            {
                res.Add(item);
            }
            return res.Select(i => new DataItem(i.RowKey, (string)i["Name"])).ToList();
        }
    }
}
