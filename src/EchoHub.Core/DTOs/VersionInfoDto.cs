using System.Xml.Serialization;

namespace EchoHub.Core.DTOs;

/// <summary>
/// Root DTO for the update manifest XML, containing all available update items.
/// </summary>
[XmlRoot("updates")]
public class UpdateManifestDto
{
    /// <summary>
    /// Gets or sets the list of available update items for different platforms.
    /// </summary>
    [XmlElement("item")]
    public List<UpdateItemDto> Items { get; set; } = [];
}

/// <summary>
/// Represents a single platform-specific update entry in the update manifest.
/// </summary>
public class UpdateItemDto
{
    /// <summary>
    /// Gets or sets the target operating system identifier (e.g. "windows", "linux").
    /// </summary>
    [XmlElement("os")]
    public string Os { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version string of this update.
    /// </summary>
    [XmlElement("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the download URL for this update.
    /// </summary>
    [XmlElement("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the changelog for this release.
    /// </summary>
    [XmlElement("changelog")]
    public string Changelog { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this update is mandatory.
    /// </summary>
    [XmlElement("mandatory")]
    public bool Mandatory { get; set; }

    /// <summary>
    /// Gets or sets the checksum information for verifying download integrity.
    /// </summary>
    [XmlElement("checksum")]
    public ChecksumDto Checksum { get; set; } = new();
}

/// <summary>
/// Represents a checksum with its algorithm and hash value for download verification.
/// </summary>
public class ChecksumDto
{
    /// <summary>
    /// Gets or sets the hashing algorithm used (e.g. "SHA256").
    /// </summary>
    [XmlAttribute("algorithm")]
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hash value.
    /// </summary>
    [XmlText]
    public string Value { get; set; } = string.Empty;
}
