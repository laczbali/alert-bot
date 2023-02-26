using AlertBot.Interactions.Clients.AWS.DynamoDb.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Net;

namespace AlertBot.Interactions.Clients.AWS.DynamoDb
{
    public class DynamoDbClient
    {
        private readonly IAmazonDynamoDB dynamoDB;
		private readonly string TableName;

		public DynamoDbClient(IAmazonDynamoDB dynamoDB)
        {
            this.dynamoDB = dynamoDB;
			this.TableName = Environment.GetEnvironmentVariable("alertbot_DynamoDbTableName") ?? throw new Exception("Env var [alertbot_DynamoDbTableName] is unset");
		}

        /// <summary>
        /// Adds a new cotnact to the DB
        /// </summary>
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public async Task AddContactAsync(string name, string number)
        {
            var newContact = new Contact { DisplayName = name, PhoneNumber = number };
            await PutItem(newContact);
        }

        /// <summary>
        /// Lists all contacts as a Name:Phone dictionary
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetContactsAsync()
        {
            var contacts = await GetAllOfTypeAsync<Contact>();
            var resultDict = new Dictionary<string, string>();
            foreach (var contact in contacts)
            {
                resultDict.Add(contact.DisplayName, contact.PhoneNumber);
            }
            return resultDict;
        }

        /// <summary>
        /// If user is below limit returns true, and increases stored usage value. If user is over limit, returns false.<b/>
        /// Rate limit is per user per day.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<bool> CheckRateLimitAndLogAsync(string userId, int limit, UsageType usageType, string messageContent, string messageTarget)
        {
            // check if we exceeded the limit
            var lookupString = TwilioUsage.GetRateIndex(userId, usageType, DateTime.UtcNow);
			var filterExpression = "contains (RateIndex, :lookup_val)";
			var filterValue = new Dictionary<string, AttributeValue>
			{
				{":lookup_val", new AttributeValue{S = lookupString} }
			};
			var scanRequest = new ScanRequest
			{
				TableName = this.TableName,
				FilterExpression = filterExpression,
				ExpressionAttributeValues = filterValue
			};

			var scanResponse = await this.dynamoDB.ScanAsync(scanRequest);
			if (scanResponse.HttpStatusCode != HttpStatusCode.OK)
				throw new Exception("Failed to query transactions");

            if(scanResponse.Items.Count >= limit)
            {
                return false;
            }

            // limit is not exceeded, log new event
            var newUsage = new TwilioUsage
            {
                Content = messageContent,
                TargetNumber = messageTarget,
                UserId = userId,
                UsageType = usageType
            };
            await PutItem(newUsage);

            return true;
        }

        private async Task PutItem(ModelBase model)
        {
            var itemRequest = new PutItemRequest
            {
                TableName = this.TableName,
                Item = model.ToDbItem()
            };

            var response = await this.dynamoDB.PutItemAsync(itemRequest);
			if (response.HttpStatusCode != HttpStatusCode.OK)
				throw new Exception("Failed to save transaction");
		}

        private async Task<ModelType> GetModelByIdAsync<ModelType>(string modelId)
			where ModelType : ModelBase
		{
            var pk = $"{typeof(ModelType).Name}@{modelId}";
			var getRequest = new GetItemRequest
			{
				TableName = this.TableName,
				Key = new Dictionary<string, AttributeValue>
				{
					{"pk", new AttributeValue{ S = pk } },
					{"sk", new AttributeValue{ S = pk } }
				}
			};

			var response = await this.dynamoDB.GetItemAsync(getRequest);
			if (response.HttpStatusCode != HttpStatusCode.OK)
				throw new Exception("Failed to query transactions");

            return ModelBase.ToModel<ModelType>(response.Item);
		}

        private async Task<IEnumerable<ModelType>> GetAllOfTypeAsync<ModelType>()
            where ModelType : ModelBase
        {
            var filterExpression = "contains (pk, :id_prefix)";
            var filterValue = new Dictionary<string, AttributeValue>
            {
                {":id_prefix", new AttributeValue{S = $"{typeof(ModelType).Name}@"} }
            };
			var scanRequest = new ScanRequest
			{
				TableName = this.TableName,
				FilterExpression = filterExpression,
				ExpressionAttributeValues = filterValue
			};

            var response = await this.dynamoDB.ScanAsync(scanRequest);
			if (response.HttpStatusCode != HttpStatusCode.OK)
				throw new Exception("Failed to query transactions");

            return response.Items.Select(item => ModelBase.ToModel<ModelType>(item));
		}
    }
}
