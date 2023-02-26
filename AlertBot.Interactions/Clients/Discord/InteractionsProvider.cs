using AlertBot.Interactions.Clients.AWS;
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
		private readonly string PhoneNumberRegex;
		private readonly int CallContentCarLenLimit;
		private readonly int TextContentCarLenLimit;
		private readonly int CallRateLimit;
		private readonly int TextRateLimit;

		public InteractionsProvider(DynamoDbClient dynamoDbClient, TwilioClient twilioClient, IHttpContextAccessor httpContextAccessor)
		{
			this.dynamoDbClient = dynamoDbClient;
			this.twilioClient = twilioClient;
			this.httpContextAccessor = httpContextAccessor;
			this.PhoneNumberRegex = Environment.GetEnvironmentVariable("alertbot_AppPhoneNumberRegex") ?? throw new Exception("Env var [alertbot_AppPhoneNumberRegex] is unset");
			this.CallContentCarLenLimit = int.Parse(Environment.GetEnvironmentVariable("alertbot_AppCallContentCarLenLimit") ?? throw new Exception("Env var [alertbot_AppCallContentCarLenLimit] is unset"));
			this.TextContentCarLenLimit = int.Parse(Environment.GetEnvironmentVariable("alertbot_AppTextContentCarLenLimit") ?? throw new Exception("Env var [alertbot_AppTextContentCarLenLimit] is unset"));
			this.CallRateLimit = int.Parse(Environment.GetEnvironmentVariable("alertbot_AppCallRateLimit") ?? throw new Exception("Env var [alertbot_AppCallRateLimit] is unset"));
			this.TextRateLimit = int.Parse(Environment.GetEnvironmentVariable("alertbot_AppTextRateLimit") ?? throw new Exception("Env var [alertbot_AppTextRateLimit] is unset"));
		}

		// TODO: implement rate limiting
		// TODO: update contact dropdown in add-contact
		
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
							Required = true,
							Choices =
								(await this.dynamoDbClient.GetContactsAsync())
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
					},
					InteractionHandler = async (Interaction interaction) =>
					{
						var contactNumber = GetOptionValue<string>(interaction, "contact");
						var contents = GetOptionValue<string>(interaction, "contents");
						if(contents.Length > this.CallContentCarLenLimit)
						{
							return $":cry: Sorry, message length must be less then {this.CallContentCarLenLimit} characters, but yours is {contents.Length} long";
						}

						await this.twilioClient.SendVoiceMessage(contents, contactNumber, GetRequestHost());
						return $":speech_balloon: Calling {contactNumber} . . .";
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

						await this.twilioClient.SendVoiceMessage(contents, number, GetRequestHost());
						return $":speech_balloon: Calling {number} . . .";
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
							Required = true,
							Choices =
								(await this.dynamoDbClient.GetContactsAsync())
								.Select(contactDict => new ApplicationCommandOptionChoice{ Name = contactDict.Key, Value = contactDict.Value } )
								.ToArray()
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
						var contactNumber = GetOptionValue<string>(interaction, "contact");
						var contents = GetOptionValue<string>(interaction, "contents");
						if(contents.Length > this.TextContentCarLenLimit)
						{
							return $":cry: Sorry, message length must be less then {this.TextContentCarLenLimit} characters, but yours is {contents.Length} long";
						}

						await this.twilioClient.SendTextMessage(contents, contactNumber);
						return $":envelope: Texting {contactNumber} . . .";
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

						await this.twilioClient.SendTextMessage(contents, number);
						return $":envelope: Texting {number} . . .";
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
