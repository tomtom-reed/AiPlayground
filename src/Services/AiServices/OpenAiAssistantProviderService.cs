using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Assistants;

/*
This class allows 
*/

namespace AiPlayground.Services.AiServices;

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class OpenAiAssistantProviderService
{
    public static OpenAIAssistant GetAssistant<T>(string id)
    {
        var astClient = MyOpenAiClient.Client.GetAssistantClient();
        var assistant = astClient.GetAssistant(id);
        // validate etc 

        return new OpenAIAssistant(assistant, typeof(T));
    }
}

#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
