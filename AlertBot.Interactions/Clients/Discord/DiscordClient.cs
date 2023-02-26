using AlertBot.Interactions.Clients.Discord.Models;
using AlertBot.Interactions.Clients.Discord.Utils.ED25519;
using AlertBot.Interactions.Utils;
using System.Text;

namespace AlertBot.Interactions.Clients.Discord
{
    public class DiscordClient
    {
        private readonly string PublicKey;
		private readonly string BotToken;
		private readonly string ClientId;
		private readonly string ApiBaseUrl;

		public DiscordClient()
        {
			this.PublicKey = Environment.GetEnvironmentVariable("alertbot_DiscordPublicKey") ?? throw new Exception("Env var [alertbot_DiscordPublicKey] is unset");
			this.BotToken = Environment.GetEnvironmentVariable("alertbot_DiscordBotToken") ?? throw new Exception("Env var [alertbot_DiscordBotToken] is unset"); 
            this.ClientId = Environment.GetEnvironmentVariable("alertbot_DiscordClientId") ?? throw new Exception("Env var [alertbot_DiscordClientId] is unset");
            this.ApiBaseUrl = Environment.GetEnvironmentVariable("alertbot_DiscordApiBaseUrl") ?? throw new Exception("Env var [alertbot_DiscordApiBaseUrl] is unset");
		}

        public async Task RegisterGlobalCommands(IEnumerable<ApplicationCommand> commands)
        {
			foreach (var item in commands)
            {
                await this.RegisterGlobalCommand(item);
            }
        }

        public async Task RegisterGlobalCommand(ApplicationCommand command)
        {
            var url = $"{this.ApiBaseUrl}/applications/{this.ClientId}/commands";
            
            var httpClient = new HttpClientWrapper();
            await httpClient.MakeRequestAsync(url, HttpMethod.Post, command, "Bot", this.BotToken);
            return;
        }

		public bool InteractionRequestIsValid(IHeaderDictionary requestHeaders, string requestBody)
        {
            var signatureHeader = requestHeaders["X-Signature-Ed25519"].FirstOrDefault();
            var timestampHeader = requestHeaders["X-Signature-Timestamp"].FirstOrDefault();

            if (signatureHeader == null || timestampHeader == null) { return false; }

            var key = HexConverter.HexToByteArray(this.PublicKey);
            var signature = HexConverter.HexToByteArray(signatureHeader);
            var timestamp = Encoding.UTF8.GetBytes(timestampHeader);
            var body = Encoding.UTF8.GetBytes(requestBody);

            var message = new List<byte>();
            message.AddRange(timestamp);
			message.AddRange(body);

			return Ed25519.Verify(signature, message.ToArray(), key);
        }
    }
}
