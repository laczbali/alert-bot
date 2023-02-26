using AlertBot.Interactions.Clients.Discord;
using AlertBot.Interactions.Clients.Discord.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AlertBot.Interactions.Controllers
{
    [ApiController]
	[Route("[controller]")]
	public class InteractionController : ControllerBase
	{
		private readonly ILogger<InteractionController> logger;
		private readonly DiscordClient discordClient;
		private readonly InteractionsProvider interactionsProvider;

		public InteractionController(
			ILogger<InteractionController> logger,
			DiscordClient discordClient,
			InteractionsProvider interactionsProvider)
		{
			this.logger = logger;
			this.discordClient = discordClient;
			this.interactionsProvider = interactionsProvider;
		}

		[HttpPost]
		public async Task<IActionResult> HandlerAsync()
		{
			// deserialize request
			var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
			Interaction requestData;
			try
			{
				requestData = JsonConvert.DeserializeObject<Interaction>(requestBody);
			}
			catch(Exception e)
			{
				logger.LogError(e, $"Deserialization failure. Request:\n{requestBody}");
				return BadRequest();
			}

			// handle verification flow
			try
			{
				if (!this.discordClient.InteractionRequestIsValid(Request.Headers, requestBody))
				{
					logger.LogWarning("Invalid interaction request");
					return this.Unauthorized();
				}
			}
			catch(Exception e)
			{
				logger.LogError(e, $"Validation failure. Request:\n{requestData}\nHeaders:\n{Request.Headers}");
				return this.Unauthorized();
			}

			if(requestData.Type == InteractionType.PING)
			{
				return this.Ok(new {type = InteractionCallBackType.PONG});
			}

			// handle interaction
			string response;
			try
			{
				response = await this.interactionsProvider.HandleInteraction(requestData);
			}
			catch(Exception e)
			{
				logger.LogError(e, $"Failed to handle interaction. Request:\n{requestData}");
				response = "Something went wrong :frowning2: Please try again later";
			}

			return this.Ok(new
			{
				type = InteractionCallBackType.CHANNEL_MESSAGE_WITH_SOURCE,
				data = new
				{
					content = response
				}
			});
		}
	}
}