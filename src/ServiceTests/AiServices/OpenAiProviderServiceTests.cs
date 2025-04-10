using AiPlayground.Services.AiServices;
using Microsoft.Extensions.DependencyInjection;

namespace AiPlayground.ServiceTests.AiServices;

public class OpenAiProviderServiceTests : TestBase
{
    // private readonly OpenAiProviderService _service;

    public OpenAiProviderServiceTests() : base()
    {
        // _service = _serviceProvider.GetRequiredService<OpenAiProviderService>();
    }

    [Fact]
    public async void TestBasicFunctionalityAsync()
    {
        var builder = new OpenAIAssistantBuilder("gpt-4o-mini")
            .AddName("Test Assistant")
            .AddDescription("Test Assistant Description")
            .AddSystemInstructions("Test Assistant Instructions")
            .SetTemperature(0.5f);
        // builder.AddResponseFormat("TestStringOutput", typeof(string));
        // builder.AddTrainingFile("TestFile.txt", File.OpenRead("TestFile.txt"));
        var assistant = builder.Build();

        var r = await assistant.GetCompletionAsync<string>("Test prompt", null);
        System.Diagnostics.Debug.Print(r);
        Assert.NotEmpty(r);

        var assistant2 = OpenAiAssistantProviderService.GetAssistant<string>(assistant.Id);
        Assert.NotNull(assistant2);
        Assert.Equal(assistant.Id, assistant2.Id);
        var r2 = await assistant2.GetCompletionAsync<string>("Hello. I am running a unit test.", null);
        System.Diagnostics.Debug.Print(r2);
        Assert.NotEmpty(r2);

        assistant.Delete();
    }
}
