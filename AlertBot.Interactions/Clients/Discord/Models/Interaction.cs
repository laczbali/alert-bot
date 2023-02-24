namespace AlertBot.Interactions.Clients.Discord.Models
{
	public class Interaction
	{
		public string Id { get; set; }
		public string ApplicationId { get; set; }
		public InteractionType Type { get; set; }
		public InteractionData Data { get; set; }
		public string GuildId { get; set; }
		public GuildMember Member { get; set; }
		public User User { get; set; }
		public string Token { get; set; }
		public int Version { get; set; }
		public Message Message { get; set; }
		public string AppPermissions { get; set; }
		public string Locale { get; set; }
		public string GuildLocale { get; set; }
	}
}
