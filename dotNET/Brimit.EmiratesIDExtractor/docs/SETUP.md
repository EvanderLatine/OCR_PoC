# Setup Instructions

## 1. Azure Resource Setup

Before running the application, set up the required Azure resources:

1. **Create Document Intelligence Resource:**
   - Go to the Azure Portal and create a new Azure AI Document Intelligence resource.
   - Note the endpoint URL and API key from the resource's Keys and Endpoint section.
   - Choose the appropriate pricing tier (F0 for free tier, S0 for production).

2. **Create Storage Account:**
   - Create an Azure Storage Account for storing training data.
   - Create a blob container in the storage account.
   - Upload your labeled training documents (PDFs/images) to the container.
   - Generate a Shared Access Signature (SAS) URL for the container with read permissions for training.

3. **Train Custom Models:**
   - Use the Document Intelligence Studio (https://documentintelligence.ai.azure.com/studio) or REST API to train models.
   - **Custom Classification Model:**
     - Prepare at least 5 samples per class (front/back/other).
     - Use the 'Custom Classifier' option in Studio.
     - Train using the blob container SAS URL.
     - Note the ClassifierModelId after successful training.
   - **Custom Extraction Model:**
     - Label fields in your training documents (e.g., name, idNumber, etc.).
     - Use the 'Custom Extraction' option in Studio.
     - Train using the blob container SAS URL.
     - Note the ExtractorModelId after successful training.
   - For programmatic training, use the Azure SDK or REST API as described in the Azure documentation.

4. **Update Configuration:**
   - Add the endpoint, API key, ClassifierModelId, and ExtractorModelId to appsettings.json or user secrets.

## 2. Configure User Secrets (Recommended for Development)

To securely store sensitive configuration like API keys, use .NET User Secrets:

```bash
# Initialize user secrets for the project
dotnet user-secrets init

# Set the API key
dotnet user-secrets set \"DocumentIntelligence:ApiKey\" \"your-api-key\"

# Verify the secret was set
dotnet user-secrets list
```

## 3. Environment Variables (Alternative)

You can also use environment variables:

```bash
# Windows PowerShell
$env:DocumentIntelligence__ApiKey=\"your-api-key\"

# Windows Command Prompt
set DocumentIntelligence__ApiKey=your-api-key

# Linux/macOS
export DocumentIntelligence__ApiKey=\"your-api-key\"
```

## 4. Run the Application

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Or run with specific environment
dotnet run --environment Development
```

## 5. Test the API

1. Open browser to https://localhost:44357/swagger for Swagger UI
2. Use the test-requests.http file with VS Code REST Client extension
3. Or use Postman/cURL as described in README.md

## 6. Logging

Logs are written to:
- Console (colored output)
- File: `logs/id-document-extractor-{date}.txt`

## 7. Azure Resources

Current configuration:
- Endpoint: https://brimit-demo-document-intelligence.cognitiveservices.azure.com/
- Classifier Model: eid_side_classifier
- Extractor Model: eid_fields_extractor

## 8. Troubleshooting

### SSL Certificate Issues
If you get SSL certificate errors:
```bash
dotnet dev-certs https --trust
```

### Port Already in Use
Change the port in Properties/launchSettings.json

### Azure API Errors
- Check your API key is correct
- Verify the endpoint URL
- Ensure models are deployed and accessible
- Verify training data and model IDs

## 9. Production Deployment

For production:
1. Use Azure Key Vault for secrets
2. Configure Application Insights for monitoring
3. Use managed identity for authentication
4. Set up proper CORS policies
5. Enable HTTPS only
6. Configure rate limiting

## 10. Performance Tuning

- Current limits (F0 tier): 20 calls per minute
- For production (S0 tier): 15 requests per second
- Consider implementing caching for repeated documents
- Use connection pooling for Azure clients
