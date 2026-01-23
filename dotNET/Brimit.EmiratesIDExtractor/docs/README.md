# ID Document Extractor POC Service

A proof-of-concept ASP.NET Core Web API service that extracts data from ID documents using Azure AI Document Intelligence. Designed generically to handle multiple document types, starting with Emirates ID.

## Features

- ✅ Generic processing for different document types via processors
- ✅ Accepts front and back photos or single PDF of ID document
- ✅ Validates input based on type
- ✅ Creates PDF from images if needed
- ✅ Classifies documents to ensure correct type
- ✅ Extracts type-specific fields (e.g., Name, ID Number, Occupation, Expiry Date, NameAR, BirthDate, BackID for Emirates ID)
- ✅ Validates confidence levels for all extracted fields
- ✅ Validates front/back ID matching for Emirates ID
- ✅ Comprehensive logging with Serilog
- ✅ Swagger UI for API documentation and testing
- ✅ Health check endpoint

## Prerequisites

- .NET 8.0 SDK
- Azure AI Document Intelligence resource
- Trained custom models for:
  - Document classification
  - Field extraction per type
- Azure Storage Account for model training data

## Configuration

Update configuration with your Azure Document Intelligence settings. Store the API key using .NET User Secrets (recommended for development).

```json
{
  \"DocumentIntelligence\": {
    \"Endpoint\": \"https://your-resource.cognitiveservices.azure.com/\",
    \"ApiKey\": \"\",
    \"ClassifierModelId\": \"your-classifier-model-id\",
    \"ExtractorModelId\": \"your-extractor-model-id\",
    \"MinClassificationConfidence\": 0.9,
    \"MinExtractionConfidence\": 0.8
  }
}
```

### Security Note

For development, use .NET User Secrets to store API keys. For production, use Azure Key Vault or environment variables.

## Running the Service

1. Clone the repository
2. Navigate to the project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Run the service:
   ```bash
   dotnet run
   ```
5. Open browser to `https://localhost:44357/swagger` for Swagger UI

## API Endpoints

### POST /api/id-document/parse/{documentType}

Parses ID document of specified type (e.g., emirates-id).

**Request:**
- Method: POST
- Content-Type: multipart/form-data
- Parameters:
  - `documentType` (path): Type of document (e.g., emirates-id)
  - `front` (required): Front side image or PDF file
  - `back` (optional): Back side image (not used if front is PDF)

**Response Codes:**
- `200 OK`: Successfully extracted data
- `206 Partial Content`: Partial extraction with warnings (missing fields or low confidence)
- `400 Bad Request`: Invalid input
- `422 Unprocessable Entity`: Invalid document or extraction failed
- `500 Internal Server Error`: Server error

**Success Response Example (emirates-id):**
```json
{
  \"response\": 200,
  \"errMsg\": \"\",
  \"data\": {
    \"name\": \"John Doe Smith\",
    \"nameAR\": \"جون دو سميث\",
    \"idNumber\": \"784-1990-1234567-8\",
    \"occupation\": \"Software Engineer\",
    \"expiryDate\": \"15-06-2028\",
    \"birthDate\": \"01-01-1990\"
  }
}
```

**Partial Success Example (206):**
```json
{
  \"response\": 206,
  \"errMsg\": \"Missing fields: Occupation; Low confidence: BirthDate (0.75)\",
  \"data\": {
    \"name\": \"John Doe Smith\",
    \"nameAR\": \"جون دو سميث\",
    \"idNumber\": \"784-1990-1234567-8\",
    \"expiryDate\": \"15-06-2028\",
    \"birthDate\": \"01-01-1990\"
  }
}
```

**Error Response Examples:**

Missing files (400):
```json
{
  \"response\": 400,
  \"errMsg\": \"Both sides required for image input\",
  \"data\": null
}
```

Invalid type (422):
```json
{
  \"response\": 422,
  \"errMsg\": \"Not a valid emirates-id\",
  \"data\": null
}
```

Missing fields (422):
```json
{
  \"response\": 422,
  \"errMsg\": \"Missing required fields: Name, Expiry Date\",
  \"data\": null
}
```

Low confidence (422):
```json
{
  \"response\": 422,
  \"errMsg\": \"Low confidence for fields: ID Number (confidence: 0.75)\",
  \"data\": null
}
```

Invalid document sides (422):
```json
{
  \"response\": 422,
  \"errMsg\": \"Document not valid: different front and back sides of Emirates ID uploaded\",
  \"data\": null
}
```

### GET /health

Health check endpoint for monitoring.

## Testing with Postman

1. Create a new POST request to `https://localhost:44357/api/id-document/parse/emirates-id`
2. In the Body tab, select `form-data`
3. Add file fields:
   - Key: `front`, Type: File, Value: Select front image or PDF
   - Key: `back`, Type: File, Value: Select back image (if not using PDF)
4. Send the request

## Testing with cURL

```bash
curl -X POST https://localhost:44357/api/id-document/parse/emirates-id \\
  -F \"front=@/path/to/front.jpg\" \\
  -F \"back=@/path/to/back.jpg\"
```

## Architecture

```
├── Controllers/
│   └── IdDocumentController.cs    # Generic API controller using factory
├── Services/
│   ├── IPdfService.cs             # PDF creation interface
│   ├── PdfService.cs              # PDF creation implementation
│   ├── IDocumentIntelligenceService.cs  # Azure AI interface
│   └── DocumentIntelligenceService.cs   # Azure AI implementation
├── Processors/
│   └── EmiratesIdProcessor.cs     # Processor for Emirates ID
├── Factories/
│   └── DocumentProcessorFactory.cs# Factory to resolve processors
├── Interfaces/
│   └── IDocumentProcessor.cs      # Processor interface
├── Models/
│   ├── ApiResponse.cs             # Standardized API response
│   ├── EmiratesIdData.cs          # Emirates ID data model
│   └── ParseDocumentRequest.cs    # Generic request model
├── Configuration/
│   └── DocumentIntelligenceOptions.cs  # Configuration model
└── Program.cs                     # Application entry point
```

## Logging

Logs are written to:
- Console (with colored output in development)
- File: `logs/id-document-extractor-{date}.txt` (JSON format)

## Performance Considerations

- Service is stateless and can be horizontally scaled
- Azure Document Intelligence F0 tier: 20 calls per minute
- For production (S0 tier): 15 requests per second
- PDF creation is done in-memory for efficiency

## Future Enhancements

- Add processors for additional document types (e.g., passport)
- Add caching for frequently processed documents
- Implement retry logic for Azure API calls
- Add support for batch processing
- Integrate with Azure Application Insights
- Add authentication/authorization
- Support for additional document types

## Troubleshooting

1. **\"Not a valid document\" error**
   - Ensure the images/PDF are clear and properly oriented
   - Check that the classifier model is properly trained

2. **\"Missing required fields\" error**
   - Verify input contains both sides
   - Check image quality and resolution

3. **\"Low confidence\" error**
   - Improve image quality
   - Ensure good lighting and no glare
   - Consider retraining the extraction model

4. **\"Document not valid: different front and back sides\" error**
   - Ensure uploaded images are from the same ID card
   - Check for extraction errors in ID numbers

## License

This is a proof-of-concept project for demonstration purposes.
