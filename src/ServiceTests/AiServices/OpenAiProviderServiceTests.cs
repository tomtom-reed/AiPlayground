using AiPlayground.Services.AiServices;
using Microsoft.Extensions.DependencyInjection;

namespace AiPlayground.ServiceTests.AiServices;

public class OpenAiProviderServiceTests : TestBase
{
    private readonly OpenAiProviderService _service;

    public OpenAiProviderServiceTests() : base()
    {
        _service = _serviceProvider.GetRequiredService<OpenAiProviderService>();
    }

    [Fact]
    public void TestBasicFunctionality()
    {
        using (Stream fileContent = File.Open(@"/FILE PATH GOES HERE", FileMode.Open))
        {
            string modelId = "asst_longstringgoeshere";
            string prompt = "Provide a summary for the attached file";

            var output = _service.GetChatFromFile(modelId, prompt, fileContent);
            
            Assert.NotEmpty(output);
            System.Diagnostics.Debug.Print(output);
        }
    }
}
