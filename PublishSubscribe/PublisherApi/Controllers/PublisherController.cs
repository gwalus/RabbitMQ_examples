using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

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

    [HttpPost]
    public IActionResult PublishMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "publish_subscribe_exchange", routingKey: "", basicProperties: null, body: body);
        return Ok();
    }
}
