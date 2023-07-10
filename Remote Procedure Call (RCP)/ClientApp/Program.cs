using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

class Program
{
    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);

    static void Main()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var replyQueueName = channel.QueueDeclare().QueueName;
            var replyConsumer = new EventingBasicConsumer(channel);
            string response = null;

            replyConsumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == replyConsumer.ConsumerTags[0])
                {
                    response = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine("Received response: {0}", response);

                    semaphore.Release(); // Zwolnienie semafora po otrzymaniu odpowiedzi
                }
            };

            channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: replyConsumer);

            Console.WriteLine("Enter your request:");
            var request = Console.ReadLine();

            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;

            var requestBytes = Encoding.UTF8.GetBytes(request);

            channel.BasicPublish(exchange: "", routingKey: "rpc_reply_queue", basicProperties: props, body: requestBytes);

            semaphore.Wait(); // Oczekiwanie na odpowiedź

            Console.WriteLine("Response received: {0}", response);
        }
    }
}