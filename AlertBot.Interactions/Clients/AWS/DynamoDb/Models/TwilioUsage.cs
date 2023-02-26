namespace AlertBot.Interactions.Clients.AWS.DynamoDb.Models
{
	public class TwilioUsage : ModelBase
	{
		public override string ModelId() => this.EventId;

		public TwilioUsage()
		{
			this.EventId = Guid.NewGuid().ToString();
			this.DateTime = DateTime.UtcNow;
		}

		public string EventId { get; set; }
		public DateTime DateTime { get; set; }
		public string RateIndex => GetRateIndex(UserId, UsageType, DateTime);

		public string UserId { get; set; }
		public UsageType UsageType { get; set; }
		public string Content { get; set; }
		public string TargetNumber { get; set; }

		public static string GetRateIndex(string userId, UsageType usageType, DateTime dateTime)
		{
			return $"{userId}@{usageType}@{dateTime.Year}-{dateTime.DayOfYear}";
		}
	}

	public enum UsageType
	{
		Text,
		Voice
	}
}
