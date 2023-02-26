using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

namespace AlertBot.Interactions.Clients.AWS.DynamoDb.Models
{
	public abstract class ModelBase
	{
		public string pk => $"{this.GetType().Name}@{ModelId()}";
		public string sk => pk;

		/// <summary>
		/// What is the unique identifier of this type of model?
		/// </summary>
		/// <returns></returns>
		public abstract string ModelId();

		public Dictionary<string, AttributeValue> ToDbItem()
		{
			var modelJson = JsonConvert.SerializeObject(this);
			var modelDoc = Document.FromJson(modelJson);
			var modelItem = modelDoc.ToAttributeMap();
			return modelItem;
		}

		public static ModelType ToModel<ModelType>(Dictionary<string, AttributeValue> dbItem)
		{
			var itemDocument = Document.FromAttributeMap(dbItem);
			return JsonConvert.DeserializeObject<ModelType>(itemDocument.ToJson());
		}
	}
}
