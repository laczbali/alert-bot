using AlertBot.Interactions.Clients.AWS;
using AlertBot.Interactions.Clients.Discord.Models;

namespace AlertBot.Interactions.Clients.Discord
{
	public class InteractionsProvider
	{
		private readonly DynamoDbClient dynamoDbClient;

		public InteractionsProvider(DynamoDbClient dynamoDbClient)
		{
			this.dynamoDbClient = dynamoDbClient;
		}

		public async Task<ApplicationCommand[]> GetGlobalCommands()
		{
			return new ApplicationCommand[]
			{
				new ApplicationCommand
				{
					Name = "add-contact",
					Description = "Add a new phone number to contacts. If it already exists, it will ovewrite it.",
					Type = ApplicationCommandType.CHAT_INPUT,
					Options = new ApplicationCommandOption[]
					{
						new ApplicationCommandOption
						{
							Name = "display-name",
							Description = "Name of the contact",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						},
						new ApplicationCommandOption
						{
							Name = "phone-number",
							Description = "Phone number in +36000000000 format",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						}
					}
				},

				new ApplicationCommand
				{
					Name = "list-contacts",
					Description = "List all contacts.",
					Type = ApplicationCommandType.CHAT_INPUT
				},

				new ApplicationCommand
				{
					Name = "call",
					Description = "Call a contact.",
					Type = ApplicationCommandType.CHAT_INPUT,
					Options = new ApplicationCommandOption[]
					{
						new ApplicationCommandOption
						{
							Name = "contact",
							Description = "Who to call",
							Type = ApplicationCommandOptionType.STRING,
							Required = true,
							Choices =
								(await this.dynamoDbClient.GetContacts())
								.Select(contactDict => new ApplicationCommandOptionChoice{ Name = contactDict.Key, Value = contactDict.Value } )
								.ToArray()
						},
						new ApplicationCommandOption
						{
							Name = "contents",
							Description = "What should the caller say",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						}
					}
				},

				//new ApplicationCommand
				//{
				//	Name = "call-number",
				//	Description = "Call a number not in contacts.",
				//	Type = ApplicationCommandType.CHAT_INPUT,
				//	Options = new ApplicationCommandOption[]
				//	{

				//	}
				//},

				//new ApplicationCommand
				//{
				//	Name = "text",
				//	Description = "Send a text message a contact.",
				//	Type = ApplicationCommandType.CHAT_INPUT,
				//	Options = new ApplicationCommandOption[]
				//	{

				//	}
				//},

				//new ApplicationCommand
				//{
				//	Name = "text-number",
				//	Description = "Send a text message to a number not in contacts.",
				//	Type = ApplicationCommandType.CHAT_INPUT,
				//	Options = new ApplicationCommandOption[]
				//	{

				//	}
				//}
			};
		}
	}
}
