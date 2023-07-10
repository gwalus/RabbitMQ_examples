using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
};

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "topic_exchange", type: ExchangeType.Topic);

    var queueName = channel.QueueDeclare().QueueName;

    var routingKeys = new string[] { "topic1.*", "*.topic2", "#.topic3" };

    foreach (var routingKey in routingKeys)
    {
        channel.QueueBind(queue: queueName, exchange: "topic_exchange", routingKey: routingKey);
    }

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine("Received message: {0}", message);
    };

    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

    Console.WriteLine("Waiting for messages...");
    Console.ReadLine();
}