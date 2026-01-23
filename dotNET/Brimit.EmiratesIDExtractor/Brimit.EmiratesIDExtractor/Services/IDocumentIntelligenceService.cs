using Azure.AI.DocumentIntelligence;

namespace Brimit.EmiratesIDExtractor.Services;

/// <summary>
/// Service for Azure Document Intelligence operations
/// </summary>
public interface IDocumentIntelligenceService
{
    /// <summary>
    /// Classifies a document to determine if it's an Emirates ID
    /// </summary>
    /// <param name="documentBytes">Document bytes to classify</param>
    /// <returns>Classification result with confidence</returns>
    Task<(bool isEmiratesId, double confidence)> ClassifyDocumentAsync(byte[] documentBytes);
    
    /// <summary>
    /// Extracts fields from an Emirates ID document
    /// </summary>
    /// <param name="documentBytes">Document bytes to analyze</param>
    /// <returns>Analyzed document result</returns>
    Task<AnalyzeResult> ExtractFieldsAsync(byte[] documentBytes);
}