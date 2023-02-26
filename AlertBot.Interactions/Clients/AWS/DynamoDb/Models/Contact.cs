namespace AlertBot.Interactions.Clients.AWS.DynamoDb.Models
{
	public class Contact : ModelBase
	{
		public override string ModelId() => DisplayName;

		public string DisplayName { get; set; }
		public string PhoneNumber { get; set; }
	}
}
