namespace AlertBot.Interactions.Clients.AWS
{
	public class DynamoDbClient
	{
		// this will handle
		// - storing rate-limiting state
		// - storing contacts

		/// <summary>
		/// Adds a new cotnact to the DB
		/// </summary>
		/// <param name="name"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		public async Task AddContactAsync(string name, string number)
		{
			return;
		}
		
		/// <summary>
		/// Lists all contacts as a Name:Phone dictionary
		/// </summary>
		/// <returns></returns>
		public async Task<Dictionary<string, string>> GetContactsAsync()
		{
			return new Dictionary<string, string>
			{
				{"billy bob", "123" },
				{"john doe", "456" }
			};
		}

		/// <summary>
		/// If user is below limit returns true, and increases stored usage value. If user is over limit, returns false.<b/>
		/// Rate limit is per user per day.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public async Task<bool> CheckRateLimit(string userId, int limit)
		{
			return true;
		}
	}
}
