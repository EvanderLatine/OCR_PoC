namespace Brimit.EmiratesIDExtractor.Configuration;

/// <summary>
/// Configuration options for Azure Document Intelligence service
/// </summary>
public class DocumentIntelligenceOptions
{
    /// <summary>
    /// The configuration section name
    /// </summary>
    public const string SectionName = "DocumentIntelligence";
    
    /// <summary>
    /// Azure Document Intelligence endpoint URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Azure Document Intelligence API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Model ID for Emirates ID classification
    /// </summary>
    public string ClassifierModelId { get; set; } = string.Empty;
    
    /// <summary>
    /// Model ID for Emirates ID field extraction
    /// </summary>
    public string ExtractorModelId { get; set; } = string.Empty;
    
    /// <summary>
    /// Minimum confidence threshold for classification (0.0 - 1.0)
    /// </summary>
    public double MinClassificationConfidence { get; set; } = 0.9;
    
    /// <summary>
    /// Minimum confidence threshold for field extraction (0.0 - 1.0)
    /// </summary>
    public double MinExtractionConfidence { get; set; } = 0.8;
}