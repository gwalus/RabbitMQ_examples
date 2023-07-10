﻿using RabbitMQ.Client;
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
    channel.QueueDeclare(queue: "work_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        Console.WriteLine("Processing message: {0}", message);

        try
        {
            SimulateMessageProcessing(message);

            Console.WriteLine("Message processed: {0}", message);

            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        catch (Exception)
        {
            channel.BasicReject(ea.DeliveryTag, requeue: true);

            Console.WriteLine("Something went wrong, message has been requeued.", message);
        }
    };

    channel.BasicConsume(queue: "work_queue", autoAck: false, consumer: consumer);

    Console.WriteLine("Waiting for messages...");
    Console.ReadLine();
}

void SimulateMessageProcessing(string message)
{
    if(message == "1")
    {
        throw new ArgumentException("Argument '1' is invalid.");
    }

    // Symulacja przetwarzania wiadomości przez odczekanie
    Thread.Sleep(10000);
}