// using System.ClientModel;
// using OpenAI;
// using OpenAI.Assistants;
// using OpenAI.Files;

// namespace AiPlayground.Services.AiServices;

// public class OpenAiProviderService {
//     private readonly OpenAIClient _client;
    
//     public OpenAiProviderService() {
//         _client = new(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
//     }

//     public string GetChatFromFile(string modelId, string prompt, Stream fileContent)
//     {
//         var fileclient = _client.GetOpenAIFileClient();
//         var filename = Guid.NewGuid();
//         OpenAIFile file = fileclient.UploadFile(fileContent, filename.ToString(), FileUploadPurpose.Assistants);

// #pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//         var assistantClient = _client.GetAssistantClient();
//         var assist = assistantClient.GetAssistant(modelId);

//         MessageContent promptMsg = MessageContent.FromText(prompt);
//         MessageCreationAttachment promptFileAttachment = new(
//             fileId: file.Id,
//             tools: [new CodeInterpreterToolDefinition()]);
//         ThreadInitializationMessage initialMsg = new(MessageRole.User, [promptMsg]);
//         initialMsg.Attachments.Add(promptFileAttachment);

//         ThreadCreationOptions threadOptions = new()
//         {
//             InitialMessages = {initialMsg}
//         };

//         ThreadRun run = assistantClient.CreateThreadAndRun(assist.Value.Id, threadOptions);

//         do
//         {
//             Thread.Sleep(TimeSpan.FromSeconds(1));
//             run = assistantClient.GetRun(run.ThreadId, run.Id);
//         } 
//         while (!run.Status.IsTerminal);

//         CollectionResult<ThreadMessage> messages = assistantClient.GetMessages(run.ThreadId);

//         string r = string.Empty;
//         System.Diagnostics.Debug.Print("?Number of messages: " + messages.Count() + "\n");
//         foreach(var msg in messages)
//         {
//             r += $"[{msg.Role.ToString()}]: ";
//             foreach (MessageContent contentItem in msg.Content)
//             {
//                 if(!string.IsNullOrEmpty(contentItem.Text))
//                 {
//                     r += $"\t{contentItem.Text}\n";

//                     // contentItem.TextAnnotations can be used for file citations etc
//                     // This code does not handle file outputs
//                 }
//             }
//             r += "\n";
//         }
//         System.Diagnostics.Debug.Print(r);
//         assistantClient.DeleteThread(run.ThreadId);
//         fileclient.DeleteFile(file.Id);
// #pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

//         return r;
//     }

//     /*public void CreateOpenAIAssistant()
//     {
//         #pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//         var assistantClient = _client.GetAssistantClient();
//         AssistantCreationOptions options = new AssistantCreationOptions();
//         options.Description = "";
//         options.Instructions = "";
//         options.Name = "";
//         options.ToolResources;
//         options.Tools;
//         var tools = new List<ToolDefinition>();
//         // var t1 = new ToolDefinition();
//         var resources = new List<ToolResources>();
//         var r1 = new ToolResources();
//         assistantClient.CreateAssistant();


//         _client.
//         #pragma warning restore OPENAI001
//     }*/
// }
