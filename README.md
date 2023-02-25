# alert-bot

It is meant to send SMS messages on command, using Twilio.

**Requirements**
- Slash command to register a new phone number `/add-contact [display-name] [phone-number]`
- Slash command to send message to a pre-registered number `/call [number-display-name] [message]`
- Rate limiting (X call + Y sms / user / day), set by env var

**Optional goals**
- Use unregistered phone numbers

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