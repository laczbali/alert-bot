namespace AlertBot.Interactions.Clients.Twilio
{
    public class TwilioClient
    {
        public async Task SendTextMessage(string contents, string targetPhoneNumber)
        {
            throw new NotImplementedException();
        }

        public async Task SendVoiceMessage(string contents, string targetPhoneNumber)
        {
            // Flow
            // 1 - A GUID gets generated
            // 2 - We upload GUID.xml to S3
            // 3 - We return this URL: azurefunction/voice/GUID.xml
            // 4 - Twilio hits a controller here (HTTP POST)
            // 5 - We fetch the file from S3, return it to Twilio and delete it from S3
			throw new NotImplementedException();
		}
    }
}
