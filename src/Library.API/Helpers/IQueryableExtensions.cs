using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Library.API.Services;

namespace Library.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if(source ==null) throw 
                new ArgumentNullException("source");

            if(mappingDictionary ==null)
                throw new ArgumentNullException("mappingDictionary");

            if (string.IsNullOrWhiteSpace(orderBy))
                return source;


            var orderByAfterSplit = orderBy.Split(",");
            foreach (var clause in orderByAfterSplit.Reverse())
            {
                
                var trimmedClause = clause.Trim();
                var orderDescending = trimmedClause.EndsWith(" desc");
                var indexOfFirstSpace = trimmedClause.IndexOf(" ", StringComparison.Ordinal);
                var propertyName = indexOfFirstSpace == -1 ? trimmedClause 
                    : trimmedClause.Remove(indexOfFirstSpace);
                if(!mappingDictionary.ContainsKey(propertyName))
                    throw new ArgumentException($"Key mapping missing for {propertyName}");

                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                    throw new ArgumentException("propertyMappingValue");

                foreach (var destination in propertyMappingValue.DestinationProperties.Reverse())
                {
                    if (propertyMappingValue.Revert)
                        orderDescending = !orderDescending;

                source = source.OrderBy(destination + (orderDescending ? " descending" : " ascending"));
                }
 
            }

            return source;
        }
    }
}
