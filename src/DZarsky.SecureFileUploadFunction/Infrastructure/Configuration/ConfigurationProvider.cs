using Microsoft.Extensions.Configuration;
using System;

namespace DZarsky.SecureFileUploadFunction.Infrastructure.Configuration
{
    public static class ConfigurationProvider
    {
        public static T GetValueFromContainer<T>(this IConfiguration configuration, string key)
        {
            var sections = key.Split('.');

            foreach (var section in sections[..(sections.Length - 1)])
            {
                configuration = configuration.GetSection(section);
            }

            var value = configuration.GetValue<T>(sections[^1]);

            if (value != null)
            {
                return value;
            }

            return (T)Convert.ChangeType(Environment.GetEnvironmentVariable(key), typeof(T));
        }
    }
}
