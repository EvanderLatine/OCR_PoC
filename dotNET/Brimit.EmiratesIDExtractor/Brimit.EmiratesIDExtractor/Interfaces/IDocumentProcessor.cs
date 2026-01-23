using Brimit.EmiratesIDExtractor.Models;

namespace Brimit.EmiratesIDExtractor.Interfaces;

public interface IDocumentProcessor
{
    string DocumentType { get; }
    Task<ApiResponse<object>> ProcessAsync(ParseDocumentRequest request);
} 