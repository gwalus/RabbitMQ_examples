using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace ProducerApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProducerController : ControllerBase
{
    private readonly ILogger<ProducerController> _logger;
    private readonly IModel _channel;

    public ProducerController(ILogger<ProducerController> logger, IModel channel)
    {
        _logger = logger;
        _channel = channel;
    }

    [HttpPost]
    public IActionResult PublishMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: "my_queue", basicProperties: null, body: body);
        return Ok();
    }
}
