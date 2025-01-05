
using MoreConvenientJiraSvn.Core.Model;
using System.ComponentModel;
using System.Reflection;

namespace MoreConvenientJiraSvn.Core.Utils;
public static class EnumHelper
{
    public static List<EnumDescription> GetEnumDescriptions<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
                   .Cast<T>()
                   .Select(e => new EnumDescription
                   {
                       Value = e,
                       Description = GetEnumDescription(e)
                   })
                   .ToList();
    }

    private static string GetEnumDescription<T>(T value) where T : notnull
    {
        FieldInfo? field = value?.GetType()?.GetField(value.ToString() ?? string.Empty);
        return field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                      .FirstOrDefault() is not DescriptionAttribute attribute
                      ? value?.ToString() ?? string.Empty
                      : attribute.Description;
    }
}