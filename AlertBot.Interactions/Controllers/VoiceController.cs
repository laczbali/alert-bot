using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace AlertBot.Interactions.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class VoiceController : Controller
	{
		[HttpGet("{message}")]
		[HttpPost("{message}")]
		public ContentResult HandlerAsync(string message)
		{
			// discard last part (.xml) & decode
			var splitMessage = message.Split(".");
			message = string.Join(".", splitMessage.Take(splitMessage.Length - 1));
			message = HttpUtility.UrlDecode(message);

			// create response XML and return
			var xmlMessage = $"<Response><Say>{message}</Say></Response>";
			return new ContentResult
			{
				Content = xmlMessage,
				ContentType = "application/xml",
				StatusCode = 200
			};
		}
	}
}
