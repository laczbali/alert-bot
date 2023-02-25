namespace AlertBot.Interactions.Clients.Discord.Models
{
	public class InteractionData
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public ApplicationCommandType Type { get; set; }
		public object Resolved { get; set; }
		public ApplicationCommandInteractionDataOption[] Options { get; set; }
		public string GuildId { get; set; }
		public string TargetId { get; set; }
	}
}
