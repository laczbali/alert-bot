namespace AlertBot.Interactions.Clients.AWS
{
	public class DynamoDbClient
	{
		// this will handle
		// - storing rate-limiting state
		// - storing contacts

		public async Task<Dictionary<string, string>> GetContacts()
		{
			return new Dictionary<string, string>
			{
				{"billy bob", "123" },
				{"john doe", "456" }
			};
		}
	}
}
