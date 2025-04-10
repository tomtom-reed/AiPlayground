
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Schema;
using OpenAI.Assistants;
using OpenAI.Files;

namespace AiPlayground.Services.AiServices;

/*
The goal of this class is to operate as an Entity Framework-like object for OpenAI Assistants.
It can be created by OpenAIAssistantBuilder or obtained through OpenAIAssistantServices
*/

/*
So let's consider this abstract, just for now.

This class is an abstract wrapper around the OpenAI Assistant API. It provides a simplified interface for interacting with the OpenAI Assistant, including methods for creating and managing threads, uploading files, and getting completions.
Notably, this class is abstract because of the response format.
Strings are useful but properly JSON formatting the response data has more use cases than simple strings. 
Asking for a list of items and then not needing to parse the response, extracting specific data, etc.
The abstract class uses generics and its implementations should be able to specify the response format.

It is important to note there can be many assistants mapping to each implementation / response format.
For example: an investment fund analysis assistant implementation could have different assistants for each fund being analyzed.
An assistant to create questions to determine customer preference about a product category could have different assistants for each product category.
Implementations are specific to the response format, which is specific to the business logic but not necessarily the use case. 
*/


#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public interface IOpenAIAssistant
{
    // View details (including audit details)
    // Update details
    // Save//?
    // Execute prompt
    // Execute prompt using file 

    /// <summary>
    /// Get a completion from the assistant.
    /// This method will upload the files to the assistant and then run the assistant with the prompt.
    /// The files will be deleted after the run is complete.
    /// 
    /// Note that the response format is generally just string unless a format was used to build the assistant.
    /// There is no validation of the response format and so it must be provided by the caller.
    /// I believe this to be a run-time vs compile-time issue and I am not happy about it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prompt"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    Task<T> GetCompletionAsync<T>(string prompt, IEnumerable<Stream>? files);

    /// <summary>
    /// Deletes the assistant. The object should be disposed after this is called.
    /// </summary>
    void Delete();
}

public class OpenAIAssistant : IOpenAIAssistant
{
    private readonly Assistant _a;
    private readonly Type? _responseType;

    public string Id { get => _a.Id; }
    
    public OpenAIAssistant(Assistant a, Type type)
    {
        _a = a;
        _responseType = type;
    }

    public async Task<T> GetCompletionAsync<T>(string prompt, IEnumerable<Stream>? files)
    {
        if (typeof(T) != typeof(string) && typeof(T) != _responseType)
        {
            throw new Exception($"Response type mismatch: {typeof(T)} != {_responseType}");
        }
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new Exception("Prompt is required");
        }
        // could check if streams are unique, but I am not.

        var promptMsg = MessageContent.FromText(prompt);
        ThreadInitializationMessage initialMsg = new(MessageRole.User, [promptMsg]);
        
        OpenAIFileClient? fileclient = null;
        List<string> fileIds = new(files != null ? files.Count() : 0);
        if (files != null && files.Count() >= 0)
        {
            fileclient = MyOpenAiClient.Client.GetOpenAIFileClient();
            var uploadTasks = new List<Task<ClientResult<OpenAIFile>>>(files.Count());
            foreach(var f in files)
            {
                uploadTasks.Add(fileclient.UploadFileAsync(f, Guid.NewGuid().ToString(), FileUploadPurpose.Assistants));
            }
            foreach (var t in await Task.WhenAll(uploadTasks))
            {
                fileIds.Add(t.Value.Id);
                MessageCreationAttachment promptFileAttachment = new(
                    fileId: t.Value.Id,
                    tools: [new FileSearchToolDefinition()]);
                initialMsg.Attachments.Add(promptFileAttachment);
            }
        }
        
        ThreadCreationOptions threadOptions = new()
        {
            InitialMessages = {initialMsg}
        };

        var assistantClient = MyOpenAiClient.Client.GetAssistantClient();

        ThreadRun run = await assistantClient.CreateThreadAndRunAsync(_a.Id, threadOptions);

        do
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            run = assistantClient.GetRun(run.ThreadId, run.Id);
        } 
        while (!run.Status.IsTerminal);

        // if(!run.Status.IsTerminal)
        // {
        //     throw new Exception("Async does not do what you thought it did");
        // }
        

        var messages = assistantClient.GetMessages(run.ThreadId);

        var r = string.Empty;
        System.Diagnostics.Debug.Print("?Number of messages: " + messages.Count() + "\n");
        foreach(var msg in messages.Where(r=> r.Role == MessageRole.Assistant))
        {
            System.Diagnostics.Debug.Print($"{msg.Role}");
            foreach (var contentItem in msg.Content.Where(r => !string.IsNullOrEmpty(r.Text)))
            {
                System.Diagnostics.Debug.Print($" : {contentItem.Text}");
                r += $"{contentItem.Text}\n";
            }
            System.Diagnostics.Debug.Print("\n");
            // contentItem.TextAnnotations can be used for file citations etc
            // This code does not handle file outputs
        }
        
        // Cleanup
        assistantClient.DeleteThread(run.ThreadId);
        if(fileclient != null)
        {
            foreach(var fileId in fileIds)
            {
                var file = fileclient.GetFile(fileId.ToString());
                if (file != null)
                {
                    fileclient.DeleteFile(fileId.ToString());
                }
            }
        }

        _a.ResponseFormat.GetType();

        if (typeof(T) == typeof(string))
        {
            return (T)(object)r;
        }

        return MapCompletionToType<T>(r);
    }

    protected T MapCompletionToType<T>(string response)
    {
        var d = JsonSerializer.Deserialize<T>(response);
        if (d == null)
        {
            System.Diagnostics.Debug.Print($"Failed to deserialize response: \n{response}");
            throw new Exception($"Failed to deserialize response:\n{response}");
        }
        return (T)d;
    }

    public void Delete()
    {
        var assistantClient = MyOpenAiClient.Client.GetAssistantClient();
        assistantClient.DeleteAssistant(_a.Id);
    }
}

#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
