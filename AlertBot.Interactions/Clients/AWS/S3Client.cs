namespace AlertBot.Interactions.Clients.AWS
{
	public class S3Client
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="contents">Raw file contents</param>
		/// <param name="fullFileName">The name of the file to store, including the extension</param>
		/// <returns>Text File URL</returns>
		public async Task<string> StoreExpiringText(string contents, string fullFileName)
		{
			// https://aws.amazon.com/blogs/aws/amazon-s3-object-expiration/
			throw new NotImplementedException();
		}
	}
}
