using Azure;
using Azure.AI.DocumentIntelligence;
using Brimit.EmiratesIDExtractor.Configuration;
using Microsoft.Extensions.Options;

namespace Brimit.EmiratesIDExtractor.Services;

/// <summary>
/// Service implementation for Azure Document Intelligence operations
/// MCP context7: Using Azure.AI.DocumentIntelligence v1.0.0 with BinaryData for document analysis
/// </summary>
public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly DocumentIntelligenceClient _client;
    private readonly DocumentIntelligenceOptions _options;
    private readonly ILogger<DocumentIntelligenceService> _logger;

    public DocumentIntelligenceService(
        IOptions<DocumentIntelligenceOptions> options,
        ILogger<DocumentIntelligenceService> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        // MCP context7: Initialize DocumentIntelligenceClient with endpoint and AzureKeyCredential
        var credential = new AzureKeyCredential(_options.ApiKey);
        _client = new DocumentIntelligenceClient(new Uri(_options.Endpoint), credential);
        
        _logger.LogInformation("DocumentIntelligenceService initialized with endpoint: {Endpoint}", _options.Endpoint);
    }

    /// <summary>
    /// Classifies a document to determine if it's an Emirates ID
    /// </summary>
    public async Task<(bool isEmiratesId, double confidence)> ClassifyDocumentAsync(byte[] documentBytes)
    {
        _logger.LogInformation("Starting document classification with model: {ModelId}", _options.ClassifierModelId);
        
        try
        {
            var content = new ClassifyDocumentOptions(_options.ClassifierModelId, BinaryData.FromBytes(documentBytes));
            var operation = await _client.ClassifyDocumentAsync(WaitUntil.Completed, content);
            AnalyzeResult result = operation.Value;

            // Check if document was classified as Emirates ID
            if (result.Documents != null && result.Documents.Count > 0)
            {
                var document = result.Documents[0]; // Type is AnalyzedDocument
                // document.Confidence is of type float?, convert to double
                float? documentConfidence = document.Confidence;
                double confidence = documentConfidence.Value;
                
                // Check document type and confidence (update "Emirates ID" to match your trained class name if different)
                var isEmiratesId = document.DocumentType == "eid" && 
                                  confidence >= _options.MinClassificationConfidence;
                
                _logger.LogInformation(
                    "Classification completed. Type: {DocumentType}, Confidence: {Confidence}, IsEmiratesId: {IsEmiratesId}", 
                    document.DocumentType, 
                    confidence, 
                    isEmiratesId);
                
                return (isEmiratesId, confidence);
            }
            
            _logger.LogWarning("No documents found in classification result");
            return (false, 0.0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document classification");
            throw;
        }
    }

    /// <summary>
    /// Extracts fields from an Emirates ID document
    /// </summary>
    public async Task<AnalyzeResult> ExtractFieldsAsync(byte[] documentBytes)
    {
        _logger.LogInformation("Starting field extraction with model: {ModelId}", _options.ExtractorModelId);
        
        try
        {
            // MCP context7: Use BinaryData for v1.0.0 API
            var documentData = BinaryData.FromBytes(documentBytes);
            
            var operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                _options.ExtractorModelId,
                documentData);
            
            var result = operation.Value;
            
            _logger.LogInformation(
                "Field extraction completed. Documents found: {DocumentCount}", 
                result.Documents?.Count ?? 0);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during field extraction");
            throw;
        }
    }
}