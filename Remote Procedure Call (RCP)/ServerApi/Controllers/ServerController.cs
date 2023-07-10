using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ServerApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ServerController : ControllerBase
{
    private readonly ILogger<ServerController> _logger;
    private readonly IModel _channel;

    public ServerController(ILogger<ServerController> logger, IModel channel)
    {
        _logger = logger;
        _channel = channel;
    }

    [HttpPost]
    public IActionResult ProcessRequest([FromBody] string request)
    {
        var replyProps = _channel.CreateBasicProperties();
        replyProps.CorrelationId = Guid.NewGuid().ToString();
        replyProps.ReplyTo = "rpc_reply_queue";

        var requestBytes = Encoding.UTF8.GetBytes(request);

        _channel.QueueDeclare(queue: "rpc_reply_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        _channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: replyProps, body: requestBytes);

        var response = WaitForResponse(replyProps.CorrelationId);
        return Ok(response);
    }

    private string WaitForResponse(string correlationId)
    {
        var replyConsumer = new EventingBasicConsumer(_channel);
        string response = null;

        replyConsumer.Received += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                response = Encoding.UTF8.GetString(ea.Body.ToArray());
            }
        };

        _channel.BasicConsume(queue: "rpc_reply_queue", autoAck: true, consumer: replyConsumer);

        while (response == null)
        {
            // Oczekiwanie na odpowiedü
        }

        return response;
    }
}
