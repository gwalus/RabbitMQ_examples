using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",

    ClientProvidedName = "Subscriber2App",
};

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "publish_subscribe_exchange", type: ExchangeType.Fanout);
    var queueName = channel.QueueDeclare().QueueName;
    channel.QueueBind(queue: queueName, exchange: "publish_subscribe_exchange", routingKey: "");

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