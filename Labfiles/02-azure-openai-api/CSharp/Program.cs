// Implicit using statements are included
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Azure;

// Add Azure OpenAI package
using Azure.AI.OpenAI;


// Build a config object and retrieve user settings.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
string? oaiEndpoint = config["AzureOAIEndpoint"];
string? oaiKey = config["AzureOAIKey"];
string? oaiDeploymentName = config["AzureOAIDeploymentName"];

if(string.IsNullOrEmpty(oaiEndpoint) || string.IsNullOrEmpty(oaiKey) || string.IsNullOrEmpty(oaiDeploymentName) )
{
    Console.WriteLine("Please check your appsettings.json file for missing or incorrect values.");
    return;
}

// Initialize the Azure OpenAI client...
var client = new OpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(oaiKey));

// System message to provide context to the model
string systemMessage = "I am a hiking enthusiast named Forest who helps people discover hikes in their area. If no area is specified, I will default to near Rainier National Park. I will then provide three suggestions for nearby hikes that vary in length. I will also share an interesting fact about the local nature on the hikes when making a recommendation.";

// Initialize messages list
var messagesList = new List<ChatRequestMessage>()
 {
     new ChatRequestSystemMessage(systemMessage),
 };

do {
    Console.WriteLine("Enter your prompt text (or type 'quit' to exit): ");
    string? inputText = Console.ReadLine();
    if (inputText == "quit") break;

    // Generate summary from Azure OpenAI
    if (inputText == null) {
        Console.WriteLine("Please enter a prompt.");
        continue;
    }
    
    Console.WriteLine("\nSending request for summary to Azure OpenAI endpoint...\n\n");

    // Add code to send request...

    // Part 1 - Without conversation context
    //var chatCompletionsOptions = new ChatCompletionsOptions
    //{
    //    Messages =
    //    {
    //        new ChatRequestSystemMessage(systemMessage),
    //        new ChatRequestUserMessage(inputText)
    //    },
    //    MaxTokens = 400,
    //    Temperature = 0.7f,
    //    DeploymentName = oaiDeploymentName
    //};

    // Part 2 - With conversation context maintained in a list
    messagesList.Add(new ChatRequestUserMessage(inputText));

    var chatCompletionsOptions = new ChatCompletionsOptions()
    {
        MaxTokens = 1200,
        Temperature = 0.7f,
        DeploymentName = oaiDeploymentName
    };

    // Add messages to the completion options
    foreach (ChatRequestMessage chatMessage in messagesList)
    {
        chatCompletionsOptions.Messages.Add(chatMessage);
    }

    // Send request to Azure OpenAI model
    ChatCompletions chatCompletions = client.GetChatCompletions(chatCompletionsOptions);

    // Return the response
    var messageContent = chatCompletions.Choices[0].Message.Content;

    // Add generated text to messages list
    messagesList.Add(new ChatRequestAssistantMessage(messageContent));

    Console.WriteLine($"Response: {messageContent}\n");
} while (true);
