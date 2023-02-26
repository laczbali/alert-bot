using AlertBot.Interactions.Utils;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Web;

namespace AlertBot.Interactions.Clients.Twilio
{
    public class TwilioClient
    {
		private readonly string BaseUrl;
		private readonly string AccountSID;
		private readonly string AuthToken;
		private readonly string SourceNumber;
		private readonly string MessagingSID;
		private readonly string RequestAuth;

		public TwilioClient()
        {
			this.BaseUrl = Environment.GetEnvironmentVariable("alertbot_TwilioBaseUrl") ?? throw new Exception("Env var [alertbot_TwilioBaseUrl] is unset");
			this.AccountSID = Environment.GetEnvironmentVariable("alertbot_TwilioAccountSID") ?? throw new Exception("Env var [alertbot_TwilioAccountSID] is unset");
			this.AuthToken = Environment.GetEnvironmentVariable("alertbot_TwilioAuthToken") ?? throw new Exception("Env var [alertbot_TwilioAuthToken] is unset");
			this.SourceNumber = Environment.GetEnvironmentVariable("alertbot_TwilioSourceNumber") ?? throw new Exception("Env var [alertbot_TwilioSourceNumber] is unset");
			this.MessagingSID = Environment.GetEnvironmentVariable("alertbot_TwilioMessagingSID") ?? throw new Exception("Env var [alertbot_TwilioMessagingSID] is unset");

			var authString = $"{this.AccountSID}:{this.AuthToken}";
			this.RequestAuth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authString));
		}

		/// <summary>
		/// Sends a Text Message request to Twilio
		/// </summary>
		/// <param name="contents">Text Message contents</param>
		/// <param name="targetPhoneNumber">Where should it send it to</param>
		/// <returns></returns>
        public async Task SendTextMessage(string contents, string targetPhoneNumber)
        {
			var url = $"{this.BaseUrl}/{this.AccountSID}/Messages.json";
			var values = new Dictionary<string, string>
			{
				{"MessagingServiceSid", this.MessagingSID },
				{"To", targetPhoneNumber },
				{"Body", contents}
			};			
			await MakeHttpRequestAsync(url, values);
		}

		/// <summary>
		/// Makes a Voice Call request to Twilio.<br/>
		/// It will get directed to ourBaseUrl/voice/MessageContents.xml
		/// </summary>
		/// <param name="contents">What should the voice call say</param>
		/// <param name="targetPhoneNumber">Where should it call</param>
		/// <param name="ourBaseUrl">Redirect URL, Twilio will look here for what message to send (eg: https://example.com)</param>
		/// <returns></returns>
        public async Task SendVoiceMessage(string contents, string targetPhoneNumber, string ourBaseUrl)
        {
			// Flow
			// 1 - URL set to Twilio is azurefunction\voice\UrlEncodedMessage.xml
			// 2 - Twilio hits a controller here, an XML file gets generated and sent to it

			var url = $"{this.BaseUrl}/{this.AccountSID}/Calls.json";
			if(ourBaseUrl.EndsWith("/"))
			{
				ourBaseUrl = ourBaseUrl.TrimEnd('/');
			}
			var encodedMessage = $"{ourBaseUrl}/voice/{HttpUtility.UrlEncode(contents)}.xml";
			var values = new Dictionary<string, string>
			{
				{"From", this.SourceNumber },
				{"To", targetPhoneNumber },
				{"Url", encodedMessage}
			};
			await MakeHttpRequestAsync(url , values);
		}

		private async Task MakeHttpRequestAsync(string url, Dictionary<string, string> content)
		{
			var client = new HttpClientWrapper();
			await client.MakeRequestAsync(
				url,
				HttpMethod.Post,
				new FormUrlEncodedContent(content),
				"Basic",
				this.RequestAuth);
		}
	}
}
