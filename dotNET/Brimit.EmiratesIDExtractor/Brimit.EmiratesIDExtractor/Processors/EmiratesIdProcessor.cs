using Azure.AI.DocumentIntelligence;
using Brimit.EmiratesIDExtractor.Configuration;
using Brimit.EmiratesIDExtractor.Interfaces;
using Brimit.EmiratesIDExtractor.Models;
using Brimit.EmiratesIDExtractor.Services;
using Microsoft.Extensions.Options;

namespace Brimit.EmiratesIDExtractor.Processors;

public class EmiratesIdProcessor : IDocumentProcessor
{
    public string DocumentType => "emirates-id";

    private readonly IPdfService _pdfService;
    private readonly IDocumentIntelligenceService _documentService;
    private readonly DocumentIntelligenceOptions _options;
    private readonly ILogger<EmiratesIdProcessor> _logger;

    public EmiratesIdProcessor(
        IPdfService pdfService,
        IDocumentIntelligenceService documentService,
        IOptions<DocumentIntelligenceOptions> options,
        ILogger<EmiratesIdProcessor> logger)
    {
        _pdfService = pdfService;
        _documentService = documentService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<object>> ProcessAsync(ParseDocumentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing {DocumentType} request", DocumentType);

            bool isPdfInput = request.Front?.ContentType == "application/pdf" || 
                              (request.Front?.FileName?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ?? false);

            byte[] pdfBytes;

            if (isPdfInput)
            {
                if (request.Back != null) return ApiResponse<object>.Error(400, "When providing PDF, do not provide back image");
                if (request.Front == null) return ApiResponse<object>.Error(400, "PDF file required");

                using var ms = new MemoryStream();
                await request.Front.CopyToAsync(ms);
                pdfBytes = ms.ToArray();
            }
            else
            {
                if (request.Front == null || request.Back == null) return ApiResponse<object>.Error(400, "Both sides required for image input");

                using var fms = new MemoryStream(); await request.Front.CopyToAsync(fms); var frontBytes = fms.ToArray();
                using var bms = new MemoryStream(); await request.Back.CopyToAsync(bms); var backBytes = bms.ToArray();

                pdfBytes = await _pdfService.CreateTwoPagePdfAsync(frontBytes, backBytes);
            }

            var (isValid, confidence) = await _documentService.ClassifyDocumentAsync(pdfBytes);
            if (!isValid) return ApiResponse<object>.Error(422, "Not a valid " + DocumentType);

            var result = await _documentService.ExtractFieldsAsync(pdfBytes);
            if (result.Documents == null || result.Documents.Count == 0) return ApiResponse<object>.Error(422, "Failed to extract data");

            var document = result.Documents[0];
            var data = ExtractFields(document);

            var (missing, lowConf) = ValidateFields(data, document);

            string partialMsg = "";
            if (missing.Any() || lowConf.Any())
            {
                if (missing.Count == 7)
                {
                    return ApiResponse<object>.Error(422, "No fields could be extracted");
                }

                if (missing.Any())
                {
                    partialMsg = $"Missing fields: {string.Join(", ", missing)}";
                }

                if (lowConf.Any())
                {
                    partialMsg += (partialMsg != "" ? "; " : "") + $"Low confidence: {string.Join(", ", lowConf)}";
                }
            }

            if (!string.IsNullOrWhiteSpace(data.IdNumber) && !string.IsNullOrWhiteSpace(data.BackID))
            {
                string transformedId = data.IdNumber.Trim().Replace("-", "");
                if (!data.BackID.Contains(transformedId))
                {
                    return ApiResponse<object>.Error(422, "Document not valid: different front and back sides of Emirates ID uploaded");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(data.IdNumber))
                {
                    return ApiResponse<object>.Error(422, "Document validation impossible: ID of document from the front side could not be extracted.");
                }

                if (string.IsNullOrWhiteSpace(data.BackID))
                {
                    return ApiResponse<object>.Error(422, "Document validation impossible: ID of document from the back side could not be extracted.");
                }
            }

            int responseCode = string.IsNullOrEmpty(partialMsg) ? 200 : 206;
            return new ApiResponse<object> { Response = responseCode, ErrMsg = partialMsg, Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {DocType}", DocumentType);
            return ApiResponse<object>.Error(500, "Internal server error");
        }
    }

    private EmiratesIdData ExtractFields(AnalyzedDocument document)
    {
        var data = new EmiratesIdData();
        if (document.Fields.TryGetValue("name", out var f) && f.FieldType == DocumentFieldType.String) data.Name = f.ValueString ?? "";
        if (document.Fields.TryGetValue("nameAR", out f) && f.FieldType == DocumentFieldType.String) data.NameAR = f.ValueString ?? "";
        if (document.Fields.TryGetValue("idNumber", out f) && f.FieldType == DocumentFieldType.String) data.IdNumber = f.ValueString ?? "";
        if (document.Fields.TryGetValue("occupation", out f) && f.FieldType == DocumentFieldType.String) data.Occupation = f.ValueString ?? "";
        if (document.Fields.TryGetValue("expiryDate", out f))
        {
            if (f.FieldType == DocumentFieldType.Date && f.ValueDate.HasValue) data.ExpiryDate = f.ValueDate.Value.ToString("dd-MM-yyyy");
            else if (f.FieldType == DocumentFieldType.String) data.ExpiryDate = f.ValueString ?? "";
        }
        if (document.Fields.TryGetValue("birthDate", out f))
        {
            if (f.FieldType == DocumentFieldType.Date && f.ValueDate.HasValue) data.BirthDate = f.ValueDate.Value.ToString("dd-MM-yyyy");
            else if (f.FieldType == DocumentFieldType.String) data.BirthDate = f.ValueString ?? "";
        }
        if (document.Fields.TryGetValue("backID", out var backField) && backField.FieldType == DocumentFieldType.String) data.BackID = backField.ValueString ?? "";
        return data;
    }

    private (List<string> missing, List<string> lowConf) ValidateFields(EmiratesIdData data, AnalyzedDocument document)
    {
        var missing = new List<string>();
        var lowConf = new List<string>();
        ValidateField(data.Name, "name", document, _options.MinExtractionConfidence, missing, lowConf);
        ValidateField(data.NameAR, "nameAR", document, _options.MinExtractionConfidence, missing, lowConf);
        ValidateField(data.IdNumber, "idNumber", document, _options.MinExtractionConfidence, missing, lowConf);
        ValidateField(data.Occupation, "occupation", document, _options.MinExtractionConfidence, missing, lowConf);
        ValidateField(data.ExpiryDate, "expiryDate", document, _options.MinExtractionConfidence, missing, lowConf);
        ValidateField(data.BirthDate, "birthDate", document, _options.MinExtractionConfidence, missing, lowConf);
        ValidateField(data.BackID, "backID", document, _options.MinExtractionConfidence, missing, lowConf);
        return (missing, lowConf);
    }

    private string ValidateIdMatch(EmiratesIdData data)
    {
        string transformedId = data.IdNumber.Trim().Replace("-", "");
        if (!data.BackID.Contains(transformedId))
        {
            return "Document not valid: different front and back sides of Emirates ID uploaded";
        }
        return string.Empty;
    }

    private void ValidateField(string value, string fieldName, AnalyzedDocument document, double minConf, List<string> missing, List<string> lowConf)
    {
        if (string.IsNullOrWhiteSpace(value)) missing.Add(fieldName);
        else if (document.Fields.TryGetValue(fieldName, out var field) && field.Confidence.HasValue && field.Confidence.Value < minConf)
            lowConf.Add($"{fieldName} ({field.Confidence.Value:F2})");
    }
} 