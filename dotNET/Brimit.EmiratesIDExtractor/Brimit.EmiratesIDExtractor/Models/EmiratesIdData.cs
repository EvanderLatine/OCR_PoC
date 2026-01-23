using System.Text.Json.Serialization;

namespace Brimit.EmiratesIDExtractor.Models;

/// <summary>
/// Emirates ID extracted data model
/// </summary>
public class EmiratesIdData
{
    /// <summary>
    /// Full name from Emirates ID
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Emirates ID number in format 784-XXXX-XXXXXXX-X
    /// </summary>
    public string IdNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Occupation/Profession
    /// </summary>
    public string Occupation { get; set; } = string.Empty;
    
    /// <summary>
    /// Expiry date in format DD-MM-YYYY
    /// </summary>
    public string ExpiryDate { get; set; } = string.Empty;

    /// <summary>
    /// Full name in Arabic from Emirates ID
    /// </summary>
    public string NameAR { get; set; } = string.Empty;

    /// <summary>
    /// Birth date in format DD-MM-YYYY
    /// </summary>
    public string BirthDate { get; set; } = string.Empty;

    /// <summary>
    /// Back ID number from the bottom of the back side
    /// </summary>
    [JsonIgnore]
    public string BackID { get; set; } = string.Empty;
}