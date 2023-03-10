using AlertBot.Interactions.Clients.AWS.DynamoDb;
using AlertBot.Interactions.Clients.AWS.DynamoDb.Models;
using AlertBot.Interactions.Clients.Discord.Models;
using AlertBot.Interactions.Clients.Twilio;
using System.Text.RegularExpressions;

namespace AlertBot.Interactions.Clients.Discord
{
    public class InteractionsProvider
	{
		private readonly DynamoDbClient dynamoDbClient;
		private readonly TwilioClient twilioClient;
		private readonly IHttpContextAccessor httpContextAccessor;
		private readonly DiscordClient discordClient;
		private readonly string PhoneNumberRegex;
		private readonly int CallContentCarLenLimit;
		private readonly int TextContentCarLenLimit;
		private readonly int CallRateLimit;
		private readonly int TextRateLimit;

		public InteractionsProvider(DynamoDbClient dynamoDbClient, TwilioClient twilioClient, IHttpContextAccessor httpContextAccessor, DiscordClient discordClient)
		{
			this.dynamoDbClient = dynamoDbClient;
			this.twilioClient = twilioClient;
			this.httpContextAccessor = httpContextAccessor;
			this.discordClient = discordClient;
			this.PhoneNumberRegex = Environment.GetEnvironmentVariable("alertbot_AppPhoneNumberRegex") ?? throw new Exception("Env var [alertbot_AppPhoneNumberRegex] is unset");
			this.CallContentCarLenLimit = int.Parse(Environment.GetEnvironmentVariable("alertbot_AppCallContentCarLenLimit") ?? throw new Exception("Env var [alertbot_AppCallContentCarLenLimit] is unset"));
			this.TextContentCarLenLimit = int.Parse(Environment.GetEnvironmentVariable("alertbot_AppTextContentCarLenLimit") ?? throw new Exception("Env var [alertbot_AppTextContentCarLenLimit] is unset"));
			this.CallRateLimit = int.Parse(Environment.GetEnvironmentVariable("alertbot_AppCallRateLimit") ?? throw new Exception("Env var [alertbot_AppCallRateLimit] is unset"));
			this.TextRateLimit = int.Parse(Environment.GetEnvironmentVariable("alertbot_AppTextRateLimit") ?? throw new Exception("Env var [alertbot_AppTextRateLimit] is unset"));
		}

		/// <summary>
		/// Gets the requested Application Command from the predefined Global Command list
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public async Task<ApplicationCommand> GetGlobalCommand(string name)
		{
			var command = (await GetGlobalCommands()).FirstOrDefault(c => c.Name == name);
			if(command == null)
			{
				throw new ArgumentException($"No command with name [{name}]");
			}
			return command;
		}
		
		/// <summary>
		/// Returns the list of predefined global interactions
		/// </summary>
		/// <returns></returns>
		public async Task<ApplicationCommand[]> GetGlobalCommands()
		{
			return new ApplicationCommand[]
			{
				new ApplicationCommand
				{
					Name = "alertbot-info",
					Description = "Displays a help page",
					Type = ApplicationCommandType.CHAT_INPUT,
					InteractionHandler = async (Interaction interaction) =>
					{
						var commands = await GetGlobalCommands();
						var commandStrings = commands.Select(c => $"`/{c.Name}`\n    {c.Description}");
						var commandString = string.Join("\n\n", commandStrings);

						var infoString = "Made by blaczko - https://github.com/laczbali/alert-bot";

						return $"{commandString}\n\n{infoString}";
					}
				},

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
					},
					InteractionHandler = async (Interaction interaction) =>
					{
						var displayName = GetOptionValue<string>(interaction, "display-name");
						var phoneNumber = GetOptionValue<string>(interaction, "phone-number");
						if(!IsPhoneNumberCorrect(phoneNumber))
						{
							return ":exclamation: Phone number format must be +36000000000";
						}

						await this.dynamoDbClient.AddContactAsync(displayName, phoneNumber);
						return $":blue_book: Added contact `{displayName}` with number `{phoneNumber}`";
					}
				},

				new ApplicationCommand
				{
					Name = "list-contacts",
					Description = "List all contacts.",
					Type = ApplicationCommandType.CHAT_INPUT,
					InteractionHandler = async (Interaction interaction) =>
					{
						var contacts = (await this.dynamoDbClient.GetContactsAsync()).OrderBy(item => item.Key).Select(item => $":telephone: `{item.Value}` :adult: {item.Key}");
						if(contacts.Count() == 0)
						{
							return ":face_with_monocle: No contacts are added yet, try using the `/add-contact` command";
						}
						return String.Join("\n", contacts);
					}
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
							Required = true
						},
						new ApplicationCommandOption
						{
							Name = "contents",
							Description = "What should the caller say",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						}
					},
					InteractionHandler = async (Interaction interaction) =>
					{
						var contactName = GetOptionValue<string>(interaction, "contact");
						var contactNumber = (await this.dynamoDbClient.GetContactsAsync()).GetValueOrDefault(contactName);
						if(contactNumber == null)
						{
							return $":cry: Sorry, but a contact of {contactName} couldn't be found";
						}
						var contents = GetOptionValue<string>(interaction, "contents");
						if(contents.Length > this.CallContentCarLenLimit)
						{
							return $":cry: Sorry, message length must be less then {this.CallContentCarLenLimit} characters, but yours is {contents.Length} long";
						}

						if(!await this.dynamoDbClient.CheckRateLimitAndLogAsync(interaction.Member.User.Id, this.CallRateLimit, UsageType.Voice, contents, contactNumber))
						{
							return $":cry: Sorry, you reached the limit of {this.CallRateLimit} calls per person per day";
						}

						await this.twilioClient.SendVoiceMessage(contents, contactNumber, GetRequestHost());
						return $":speech_balloon: Calling {contactNumber} . . .\n> {contents}";
					}
				},

				new ApplicationCommand
				{
					Name = "call-number",
					Description = "Call a number not in contacts.",
					Type = ApplicationCommandType.CHAT_INPUT,
					Options = new ApplicationCommandOption[]
					{
						new ApplicationCommandOption
						{
							Name = "phone-number",
							Description = "Phone number in +36000000000 format",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						},
						new ApplicationCommandOption
						{
							Name = "contents",
							Description = "What should the caller say",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						}
					},
					InteractionHandler = async (Interaction interaction) =>
					{
						var number = GetOptionValue<string>(interaction, "phone-number");
						if(!IsPhoneNumberCorrect(number))
						{
							return ":exclamation: Phone number format must be +36000000000";
						}
						var contents = GetOptionValue<string>(interaction, "contents");
						if(contents.Length > this.CallContentCarLenLimit)
						{
							return $":cry: Sorry, message length must be less then {this.CallContentCarLenLimit} characters, but yours is {contents.Length} long";
						}

						if(!await this.dynamoDbClient.CheckRateLimitAndLogAsync(interaction.Member.User.Id, this.CallRateLimit, UsageType.Voice, contents, number))
						{
							return $":cry: Sorry, you reached the limit of {this.CallRateLimit} calls per person per day";
						}

						await this.twilioClient.SendVoiceMessage(contents, number, GetRequestHost());
						return $":speech_balloon: Calling {number} . . .\n> {contents}";
					}
				},

				new ApplicationCommand
				{
					Name = "text",
					Description = "Send a text message to a contact.",
					Type = ApplicationCommandType.CHAT_INPUT,
					Options = new ApplicationCommandOption[]
					{
						new ApplicationCommandOption
						{
							Name = "contact",
							Description = "Who to send it to",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						},
						new ApplicationCommandOption
						{
							Name = "contents",
							Description = "What should the message say",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						},
					},
					InteractionHandler = async (Interaction interaction) =>
					{
						var contactName = GetOptionValue<string>(interaction, "contact");
						var contactNumber = (await this.dynamoDbClient.GetContactsAsync()).GetValueOrDefault(contactName);
						if(contactNumber == null)
						{
							return $":cry: Sorry, but a contact of {contactName} couldn't be found";
						}
						var contents = GetOptionValue<string>(interaction, "contents");
						if(contents.Length > this.TextContentCarLenLimit)
						{
							return $":cry: Sorry, message length must be less then {this.TextContentCarLenLimit} characters, but yours is {contents.Length} long";
						}

						if(!await this.dynamoDbClient.CheckRateLimitAndLogAsync(interaction.Member.User.Id, this.TextRateLimit, UsageType.Text, contents, contactNumber))
						{
							return $":cry: Sorry, you reached the limit of {this.TextRateLimit} calls per person per day";
						}

						await this.twilioClient.SendTextMessage(contents, contactNumber);
						return $":envelope: Texting {contactNumber} . . .\n> {contents}";
					}
				},

				new ApplicationCommand
				{
					Name = "text-number",
					Description = "Send a text message to a number not in contacts.",
					Type = ApplicationCommandType.CHAT_INPUT,
					Options = new ApplicationCommandOption[]
					{
						new ApplicationCommandOption
						{
							Name = "phone-number",
							Description = "Phone number in +36000000000 format",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						},
						new ApplicationCommandOption
						{
							Name = "contents",
							Description = "What should the message say",
							Type = ApplicationCommandOptionType.STRING,
							Required = true
						}
					},
					InteractionHandler = async (Interaction interaction) =>
					{
						var number = GetOptionValue<string>(interaction, "phone-number");
						if(!IsPhoneNumberCorrect(number))
						{
							return ":exclamation: Phone number format must be +36000000000";
						}
						var contents = GetOptionValue<string>(interaction, "contents");
						if(contents.Length > this.TextContentCarLenLimit)
						{
							return $":cry: Sorry, message length must be less then {this.TextContentCarLenLimit} characters, but yours is {contents.Length} long";
						}

						if(!await this.dynamoDbClient.CheckRateLimitAndLogAsync(interaction.Member.User.Id, this.TextRateLimit, UsageType.Text, contents, number))
						{
							return $":cry: Sorry, you reached the limit of {this.TextRateLimit} calls per person per day";
						}

						await this.twilioClient.SendTextMessage(contents, number);
						return $":envelope: Texting {number} . . .\n> {contents}";
					}
				}
			};
		}

		/// <summary>
		/// Matches the provided interaction to a defined Global Command, and then calls its InteractionHandler function
		/// </summary>
		/// <param name="interaction"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public async Task<string> HandleInteraction(Interaction interaction)
		{
			var registeredCommand = (await GetGlobalCommands()).FirstOrDefault(c => c.Name == interaction.Data.Name);
			if (registeredCommand == null)
			{
				throw new Exception($"Failed to find mathcing Global Command to interaction named [{interaction.Data.Name}]");
			}
			if (registeredCommand.InteractionHandler == null)
			{
				throw new Exception($"No interaction handler defined for [{registeredCommand.Name}]");
			}

			return await registeredCommand.InteractionHandler(interaction);
		}
		
		/// <summary>
		/// Gets the value of the desired interaction option
		/// </summary>
		/// <typeparam name="OptionType"></typeparam>
		/// <param name="interaction"></param>
		/// <param name="optionName"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public OptionType GetOptionValue<OptionType>(Interaction interaction, string optionName)
		{
			var option = interaction.Data.Options.FirstOrDefault(o => o.Name == optionName);
			if(option == null)
			{
				throw new ArgumentException($"Cannot find [{optionName}]");
			}

			return (OptionType)option.Value;
		}

		/// <summary>
		/// Matches the phone number to the PhoneNumberRegex env var, returns true if it matches, false otherwise
		/// </summary>
		/// <param name="phoneNumber"></param>
		/// <returns></returns>
		public bool IsPhoneNumberCorrect(string phoneNumber)
		{
			var regex = new Regex(this.PhoneNumberRegex);
			return regex.IsMatch(phoneNumber);
		}

		/// <summary>
		/// Returns request "SCHEMA://HOST"
		/// </summary>
		/// <returns></returns>
		private string GetRequestHost() => $"{this.httpContextAccessor.HttpContext.Request.Scheme}://{this.httpContextAccessor.HttpContext.Request.Host.Host}";
	}
}
