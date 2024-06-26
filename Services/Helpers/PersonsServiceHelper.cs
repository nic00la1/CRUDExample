using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ServiceContracts.Enums;

namespace Services.Helpers;

public class PersonsServiceHelper
{
    public List<T> SortByProperty<T>(
        List<T> items,
        string propertyName,
        SortOderOptions sortOrder
    )
    {
        PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public |
            BindingFlags.Instance);

        if (propertyInfo == null)
            throw new ArgumentException(
                $"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        // Check if the property is of type string
        bool isStringProperty = propertyInfo.PropertyType == typeof(string);

        return sortOrder switch
        {
            SortOderOptions.ASC => isStringProperty
                ? items.OrderBy(
                    item => propertyInfo.GetValue(item, null) as string,
                    StringComparer.OrdinalIgnoreCase).ToList()
                : items.OrderBy(item => propertyInfo.GetValue(item, null))
                    .ToList(),
            SortOderOptions.DESC => isStringProperty
                ? items.OrderByDescending(
                    item => propertyInfo.GetValue(item, null) as string,
                    StringComparer.OrdinalIgnoreCase).ToList()
                : items.OrderByDescending(item =>
                    propertyInfo.GetValue(item, null)).ToList(),
            _ => items
        };
    }
}
