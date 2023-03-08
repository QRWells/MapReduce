using System.Collections;

namespace QRWells.MapReduce.Rpc.Extensions;

public static class ObjectExtensions
{
    public static T ToObject<T>(this IDictionary<string, object> source)
        where T : class, new()
    {
        var someObject = new T();
        var someObjectType = someObject.GetType();

        foreach (var item in source)
        {
            var propertyInfo = someObjectType.GetProperty(item.Key);
            propertyInfo?.SetValue(someObject, Convert.ChangeType(item.Value, propertyInfo.GetType()), null);
        }

        return someObject;
    }

    public static dynamic ToObject(this IDictionary<string, object?>? source, Type type)
    {
        var someObject = Activator.CreateInstance(type);
        var someObjectType = someObject.GetType();

        foreach (var item in source)
        {
            var propertyInfo = someObjectType.GetProperty(item.Key)!;

            if (propertyInfo.PropertyType.IsEnum)
            {
                propertyInfo.SetValue(someObject, Enum.ToObject(propertyInfo.PropertyType, item.Value));
                continue;
            }

            if (item.Value is null || item.Value.GetType().GetInterface("IConvertible") != null)
            {
                propertyInfo.SetValue(someObject, Convert.ChangeType(item.Value, propertyInfo.PropertyType));
                continue;
            }

            propertyInfo.SetValue(someObject, RegulateObject(propertyInfo.PropertyType, item.Value));
        }

        return someObject;
    }

    public static dynamic RegulateObject(Type type, object? obj)
    {
        if (type.GetInterface("IEnumerable") != null && type.IsGenericType)
        {
            var elementType = type.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var item in (IEnumerable)obj!) list.Add(RegulateObject(elementType, item));

            return list;
        }

        if (type.GetInterface("IDictionary") != null && type.IsGenericType)
        {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            var listType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var list = (IDictionary)Activator.CreateInstance(listType)!;
            foreach (DictionaryEntry item in (IDictionary)obj!)
                list.Add(RegulateObject(keyType, item.Key), RegulateObject(valueType, item.Value));

            return list;
        }

        if (type.Name == "KeyValuePair`2")
        {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            var key = RegulateObject(keyType, type.GetProperty("Key")!.GetValue(obj));
            var value = RegulateObject(valueType, type.GetProperty("Value")!.GetValue(obj));
            var listType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
            return KeyValuePair.Create(key, value);
        }

        return obj;
    }
}