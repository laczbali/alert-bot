using AlertBot.Interactions.Clients.Discord.Utils.ED25519;
using System.Text;

namespace AlertBot.Interactions.Clients.Discord
{
    public class DiscordClient
    {
        private readonly string PublicKey;

        public DiscordClient()
        {
			this.PublicKey = Environment.GetEnvironmentVariable("alertbot_DiscordPublicKey") ?? throw new Exception("Env var [alertbot_DiscordPublicKey] is unset");
		}

        public async Task RegisterGlobalCommands()
        {
            
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
