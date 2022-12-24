using Confluent.Kafka;

namespace SomeShop.Common.App.Kafka;

public static class ConsumerExtensions
{
    public static async ValueTask<ConsumeResult<TKey, TValue>> ConsumeAsync<TKey, TValue>(
        this IConsumer<TKey, TValue> consumer, CancellationToken ct)
    {
        var res = consumer.Consume(0);
        if (res != null)
        {
            return res;
        }

        return await Task.Run(() => consumer.Consume(ct), ct);
    }
}