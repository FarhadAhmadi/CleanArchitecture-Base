using System.Reflection;

namespace Web.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SanitizeTextAttribute(int maxLength) : Attribute
{
    public int MaxLength { get; } = maxLength;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SanitizeIdentifierAttribute(int maxLength) : Attribute
{
    public int MaxLength { get; } = maxLength;
}

public static class RequestSanitizationEndpointFilter
{
    public static EndpointFilterDelegate Create(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        List<ArgumentSanitizer> argumentSanitizers = BuildArgumentSanitizers(context.MethodInfo);
        if (argumentSanitizers.Count == 0)
        {
            return next;
        }

        return async invocationContext =>
        {
            foreach (ArgumentSanitizer sanitizer in argumentSanitizers)
            {
                object? argument = invocationContext.Arguments[sanitizer.ArgumentIndex];
                if (argument is null)
                {
                    continue;
                }

                sanitizer.Sanitize(argument);
            }

            return await next(invocationContext);
        };
    }

    private static List<ArgumentSanitizer> BuildArgumentSanitizers(MethodInfo methodInfo)
    {
        List<ArgumentSanitizer> result = [];
        ParameterInfo[] parameters = methodInfo.GetParameters();

        for (int i = 0; i < parameters.Length; i++)
        {
            List<PropertySanitizer> propertySanitizers = BuildPropertySanitizers(parameters[i].ParameterType);
            if (propertySanitizers.Count == 0)
            {
                continue;
            }

            result.Add(new ArgumentSanitizer(i, propertySanitizers));
        }

        return result;
    }

    private static List<PropertySanitizer> BuildPropertySanitizers(Type type)
    {
        List<PropertySanitizer> result = [];

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.PropertyType != typeof(string) || property.GetMethod is null || property.SetMethod is null)
            {
                continue;
            }

            SanitizeTextAttribute? textAttribute = property.GetCustomAttribute<SanitizeTextAttribute>();
            if (textAttribute is not null)
            {
                result.Add(new PropertySanitizer(property, value => InputSanitizer.SanitizeText(value, textAttribute.MaxLength)));
                continue;
            }

            SanitizeIdentifierAttribute? identifierAttribute = property.GetCustomAttribute<SanitizeIdentifierAttribute>();
            if (identifierAttribute is not null)
            {
                result.Add(new PropertySanitizer(property, value => InputSanitizer.SanitizeIdentifier(value, identifierAttribute.MaxLength)));
            }
        }

        return result;
    }

    private sealed record PropertySanitizer(PropertyInfo Property, Func<string?, string?> Sanitize)
    {
        public void Apply(object target)
        {
            string? current = (string?)Property.GetValue(target);
            string? sanitized = Sanitize(current);
            if (!string.Equals(current, sanitized, StringComparison.Ordinal))
            {
                Property.SetValue(target, sanitized);
            }
        }
    }

    private sealed record ArgumentSanitizer(int ArgumentIndex, List<PropertySanitizer> PropertySanitizers)
    {
        public void Sanitize(object argument)
        {
            foreach (PropertySanitizer propertySanitizer in PropertySanitizers)
            {
                propertySanitizer.Apply(argument);
            }
        }
    }
}
