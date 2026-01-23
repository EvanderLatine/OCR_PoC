using Brimit.EmiratesIDExtractor.Interfaces;
using Brimit.EmiratesIDExtractor.Processors;

namespace Brimit.EmiratesIDExtractor.Factories;

public class DocumentProcessorFactory : IDocumentProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _processors = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
    {
        ["emirates-id"] = typeof(EmiratesIdProcessor)
    };

    public DocumentProcessorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDocumentProcessor GetProcessor(string documentType)
    {
        if (!_processors.TryGetValue(documentType, out var processorType))
        {
            throw new ArgumentException($"Unsupported document type: {documentType}");
        }

        return (IDocumentProcessor)_serviceProvider.GetRequiredService(processorType);
    }
}

public interface IDocumentProcessorFactory
{
    IDocumentProcessor GetProcessor(string documentType);
} 