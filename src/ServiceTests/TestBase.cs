using AiPlayground.Services.AiServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Services;

namespace AiPlayground.ServiceTests;

public abstract class TestBase
{
    protected readonly IServiceProvider _serviceProvider;

    public TestBase()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<OpenAiProviderService>(new OpenAiProviderService());

        _serviceProvider = builder.Services.BuildServiceProvider();
    }
}
