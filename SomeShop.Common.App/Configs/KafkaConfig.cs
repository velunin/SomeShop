namespace SomeShop.Common.App.Configs;

public class KafkaConfig
{
    public const string Section = "Kafka";
    public string BootstrapServers { get; set; }
    //Hack for auto-create topic case
    public bool IgnoreFirstUnknownTopicException { get; set; }
}