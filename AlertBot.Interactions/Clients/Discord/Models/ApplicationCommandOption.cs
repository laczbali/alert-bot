namespace AlertBot.Interactions.Clients.Discord.Models
{
	public class ApplicationCommandOption
	{
		public ApplicationCommandOptionType Type { get; set; }
		public string Name { get; set; }
		public object NameLocalizations { get; set; }
		public string Description { get; set; }
		public object DescriptionLocalizations { get; set; }
		public bool Required { get; set; }
		public ApplicationCommandOptionChoice[] Choices { get; set; }
		public ApplicationCommandOption[] Options { get; set; }
		public ChannelType[] ChannelTypes { get; set; }
		public object MinValue { get; set; }
		public object MaxValue { get; set; }
		public int MinLength { get; set; }
		public int MaxLength { get; set; }
		public bool AutoComplete { get; set; }
	}
}
