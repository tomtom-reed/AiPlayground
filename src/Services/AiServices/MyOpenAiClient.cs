using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;

namespace AiPlayground.Services.AiServices;

/// <summary>
/// Injectable OpenAIClient. This is probably wrong, I should just inject the client directly.
/// </summary>
public class MyOpenAiClient
{
    public static readonly OpenAIClient Client;
    
    static MyOpenAiClient()
    {
        Client = new(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
    }
}
