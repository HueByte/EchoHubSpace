using System.Xml.Serialization;

namespace EchoHub.App.DTOs;

[XmlRoot("updates")]
public class UpdateManifestDto
{
    [XmlElement("item")]
    public List<UpdateItemDto> Items { get; set; } = [];
}

public class UpdateItemDto
{
    [XmlElement("os")]
    public string Os { get; set; } = string.Empty;

    [XmlElement("version")]
    public string Version { get; set; } = string.Empty;

    [XmlElement("url")]
    public string Url { get; set; } = string.Empty;

    [XmlElement("changelog")]
    public string Changelog { get; set; } = string.Empty;

    [XmlElement("mandatory")]
    public bool Mandatory { get; set; }

    [XmlElement("checksum")]
    public ChecksumDto Checksum { get; set; } = new();
}

public class ChecksumDto
{
    [XmlAttribute("algorithm")]
    public string Algorithm { get; set; } = string.Empty;

    [XmlText]
    public string Value { get; set; } = string.Empty;
}
