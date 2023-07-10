using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace PublisherApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PublisherController : ControllerBase
{
    private readonly ILogger<PublisherController> _logger;
    private readonly IModel _channel;

    public PublisherController(ILogger<PublisherController> logger, IModel channel)
    {
        _logger = logger;
        _channel = channel;
    }

    [HttpPost("{routingKey}")]
    public IActionResult PublishMessage(string routingKey, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "topic_exchange", routingKey: routingKey, basicProperties: null, body: body);
        return Ok();
    }
}
