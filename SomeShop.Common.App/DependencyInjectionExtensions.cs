using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SomeShop.Common.App.Configs;
using SomeShop.Common.App.Kafka;

namespace SomeShop.Common.App;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfigs(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddConfig<PostgresConfig>(config, PostgresConfig.Section)
            .AddConfig<KafkaConfig>(config, KafkaConfig.Section);
    }

    public static IServiceCollection AddKafka(this IServiceCollection services,
        Action<IRegistryConfigurator> configure)
    {
        var registry = new ConsumersRegistry();
        configure(registry);
        services.TryAddSingleton<IConsumersRegistry>(registry);
        services.TryAddSingleton<IConsumerRunner, ConsumerRunner>();
        services.TryAddSingleton<IProducer, Producer>();

        foreach (var consumer in registry.Consumers)
        {
            services.TryAddScoped(consumer.ConsumerType);
        }

        return services;
    }

    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        return services.AddSingleton<IDomainEventsProcessor, DomainEventsProcessor>();
    }

    private static IServiceCollection AddConfig<TConfig>(this IServiceCollection services, IConfiguration config,
        string section) where TConfig : class
    {
        var configModel = config.GetRequiredSection(section).Get<TConfig>() ??
                          throw new AppConfigurationException(section, typeof(TConfig));

        return services.AddSingleton(configModel);
    }
}

public class AppConfigurationException : Exception
{
    public AppConfigurationException(string section, Type configType) : base(
        string.Format($"Unable to load config section '{section}' into '{configType.FullName}'"))
    {
    }
}