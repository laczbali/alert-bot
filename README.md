# alert-bot

It is meant to send SMS messages or voice calls on command, using Twilio.

**Features**
- Slash commands
  - Help page `/alertbot-info`
  - Register a new phone number `/add-contact [display-name] [phone-number]`
  - List contacts `/list-contacts`
  - Send a voice message to a pre-registered number `/call [number-display-name] [message]`
  - Send a text message to a pre-registered number `/text [number-display-name] [message]`
  - Send a voice message to a number `/call-number [number] [message]`
  - Send a text message to a number `/text-number [number] [message]`
- Rate limiting (X call + Y sms / user / day), set by env var

**Optional goals**
- React to messages (eg if person X is mentioned in a message, send a text message)

**Known issues**
- Punctuation mark can cause issues in calls in some cases

# Developer Guide
**Prerequisites**
- Visual Studio with web development tools installed
- Install AWS tools with `dotnet tool install -g Amazon.Lambda.Tools`
- Install the AWS CLI
- Set up AWS CLI profile with `aws configure`
  - Generate an AWS Access Key in IAM
  - Set the default region to `us-east-1`
- Create and set up a Discord Bot
  - Set it to private
  - Add the interaction endpoint (you need to publish the app first to AWS) `[AWS_URL]/interaction`
- Install and configure ngrok
- Create a table in DynamoDB
  - Partition key: "pk", Sort key: "sk"
  - Custom settings
    - DynamoDB standard
    - On-demand
    - Owned by Amazon DynamoDB
  - Set up an IAM user with AmazonDynamoDBFullAccess persmission
  - Create an access key for the user
- Set the following env vars in Properties\launchSettings.json
  - alertbot_AppPhoneNumberRegex
  - alertbot_AppCallContentCarLenLimit
  - alertbot_AppTextContentCarLenLimit
  - alertbot_AppCallRateLimit
  - alertbot_AppTextRateLimit
  - alertbot_DiscordInviteUrl
  - alertbot_DiscordApiBaseUrl
  - alertbot_DiscordPublicKey
  - alertbot_DiscordBotToken
  - alertbot_DiscordClientId
  - alertbot_TwilioBaseUrl
  - alertbot_TwilioAccountSID
  - alertbot_TwilioAuthToken
  - alertbot_TwilioSourceNumber
  - alertbot_TwilioMessagingSID
  - alertbot_DynamoDbAccessKey
  - alertbot_DynamoDbSecretKey
  - alertbot_DynamoDbTableName

**Development**
- Deploy with `dotnet lambda deploy-function` from the project (not solution) directory
- If you are doing a first-time publish:
  - Set the function name to `AlertBot`
  - Create a new IAM Role `AlertBotRole`
  - Select IAM Policy `6 - Basic Execution`
- Start the app
- Start ngrok `ngrok http [HTTP_PORT]`
- Set the ngrok URL as the interaction endpoint URL

**Notes**
- Project is based on the `Lambda ASP.NET Core Minimal API` AWS template (install templates with: `dotnet new -i Amazon.Lambda.Templates`)
  - `serverless.template` file was deleted
  - `aws-lambda-tools-defaults.json` file was edited
  - In `Startup.cs` the `AddAWSLambdaHosting` was set to use `LambdaEventSource.HttpApi`
- Thanks to [Discord.Net](https://github.com/discord-net/Discord.Net) for the verification implementation
