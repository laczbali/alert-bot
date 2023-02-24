namespace AlertBot.Interactions.Clients.Discord.Models
{
	public class ApplicationCommandInteractionDataOption
	{
		public string Name { get; set; }
		public ApplicationCommandOptionType Type { get; set; }
		public object Value { get; set; }
		public ApplicationCommandInteractionDataOption[] Options { get; set; }
		public bool Focused { get; set; }
	}
}
