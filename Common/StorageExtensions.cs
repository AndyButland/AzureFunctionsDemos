namespace Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;

    [ExcludeFromCodeCoverage] // Excluded due to hard dependencies on Azure storage components
    public static class StorageExtensions
    {
        /// <summary>
        /// Provides an extensions method to execute a query asynchronously and get all results
        /// </summary>
        /// <remarks>
        /// Hat-tip: https://stackoverflow.com/a/24270388/489433
        /// </remarks>
        public static async Task<IList<T>> ExecuteQueryAsync<T>(this CloudTable table, 
                                                                TableQuery<T> query, 
                                                                CancellationToken ct = default(CancellationToken), 
                                                                Action<IList<T>> onProgress = null) 
            where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync(query, token, ct);
                token = seg.ContinuationToken;
                items.AddRange(seg);
                onProgress?.Invoke(items);
            }
            while (token != null && !ct.IsCancellationRequested);

            return items;
        }
    }
}
