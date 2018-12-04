using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string,PropertyMappingValue> _authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id",new PropertyMappingValue(new List<string> {"Id"}) },
                {"Genre",new PropertyMappingValue(new List<string> {"Genre"}) },
                {"Age",new PropertyMappingValue(new List<string>{"DateOfBirth"},true) },
                {"Name", new PropertyMappingValue(new List<string>{"FirstName","LastName"}) }
            };

        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
            {
            propertyMappings.Add(new PropertyMapping<AuthorDto,Author>(_authorPropertyMapping));
            }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
            if (matchingMapping.Count() == 1)
                return matchingMapping.First()._mappingDictionary;

            throw  new Exception("Can not find exact property mapping instance");

        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();
            if (string.IsNullOrWhiteSpace(fields))
                return true;

            foreach (var field in fields.Split(","))
            {
                var trimmedField = field.Trim();
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
            }
            return true;
        }
        }

  

  
}
