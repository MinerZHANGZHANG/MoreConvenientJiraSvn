using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MoreConvenientJiraSvn.Core.Interfaces;
using System.Net.Http;
using System.Text;

namespace MoreConvenientJiraSvn.Service;

public class SemanticKernelService(IRepository repository, LogService logService)
{
    public Kernel BuildKernelWithChatCompletion(string baseAddress, string modelId, string apiKey, IEnumerable<string>? replaceToEmptyStrings = null)
    {
        if (Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Invalid base address", nameof(baseAddress));
        }

        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: apiKey,
                httpClient: new HttpClient(new LoggingHandler(logService, replaceToEmptyStrings ?? [], new HttpClientHandler()))
                {
                    BaseAddress = uri,
                }
        );
        builder.Services.AddLogging(logBuilder => logBuilder.AddConsole().SetMinimumLevel(LogLevel.Trace));

        Kernel kernel = builder.Build();
        return kernel;
    }

    public void RecordChatHistory(ChatHistory history)
    {
        // Todo: transform to a more convenient format

        repository.Insert(history);
    }
}


public class LoggingHandler(LogService logService, IEnumerable<string> replaceToEmptyStrings, HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        logService.LogDebug($"Request: {request.Method} {request.RequestUri}");
        if (request.Content != null)
        {
            var requestBodyContent = await request.Content.ReadAsStringAsync(cancellationToken);
            foreach (var item in replaceToEmptyStrings)
            {
                requestBodyContent = requestBodyContent.Replace(item, string.Empty);
            }
            var content = new StringContent(requestBodyContent, Encoding.UTF8, "application/json");

            request.Content = content;
            logService.LogDebug($"Request Body: {await content.ReadAsStringAsync(cancellationToken)}");
        }

        var response = await base.SendAsync(request, cancellationToken);

        logService.LogDebug($"Response: {response.StatusCode}");
        if (response.Content != null)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            logService.LogDebug($"Response Body: {responseBody}");
        }

        return response;
    }
}
