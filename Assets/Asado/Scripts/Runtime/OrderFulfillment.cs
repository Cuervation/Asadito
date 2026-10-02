using System;
using System.Collections.Generic;
namespace Asadito.Runtime
{
    public sealed class OrderFulfillmentResult
    {
        public int RequiredCount, MatchedCount;
        public readonly Dictionary<string,int> MissingByFood = new Dictionary<string,int>(StringComparer.Ordinal);
        public readonly Dictionary<string,int> UnexpectedByFood = new Dictionary<string,int>(StringComparer.Ordinal);
        public bool IsComplete => RequiredCount > 0 && MissingByFood.Count == 0 && UnexpectedByFood.Count == 0;
    }
    /// <summary>Exact requested identities/counts, independent of preference, cooking, stock and scores.</summary>
    public static class OrderFulfillment
    {
        public static OrderFulfillmentResult Evaluate(IReadOnlyList<string> requested, IReadOnlyList<string> served)
        {
            var result = new OrderFulfillmentResult();
            if (requested != null)
                foreach (var id in requested)
                {
                    result.RequiredCount++;
                    string key = id ?? string.Empty;
                    result.MissingByFood.TryGetValue(key,out int count);
                    result.MissingByFood[key] = count+1;
                }
            if (served != null)
                foreach (var id in served)
                {
                    string key = id ?? string.Empty;
                    if (!string.IsNullOrEmpty(key) && result.MissingByFood.TryGetValue(key,out int missing) && missing > 0)
                    {
                        result.MatchedCount++;
                        if (missing == 1) result.MissingByFood.Remove(key);
                        else result.MissingByFood[key] = missing-1;
                    }
                    else
                    {
                        result.UnexpectedByFood.TryGetValue(key,out int unexpected);
                        result.UnexpectedByFood[key] = unexpected+1;
                    }
                }
            return result;
        }
    }
}
