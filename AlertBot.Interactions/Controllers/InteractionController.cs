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

		public InteractionController(
			ILogger<InteractionController> logger,
			DiscordClient discordClient)
		{
			this.logger = logger;
			this.discordClient = discordClient;
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
				response = await HandleInteraction(requestData);
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="interactionName"></param>
		/// <returns>Response to send to the user</returns>
		private async Task<string> HandleInteraction(Interaction interaction)
		{
			return "ok " + interaction.Data.Name;
			//switch(interaction.Data.Name)
			//{
			//	default: throw new ArgumentException($"Unknown interaction [{interactionName}]");
			//}
		}
	}
}