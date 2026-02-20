using System.Xml.Serialization;

namespace EchoHub.App.DTOs;

[XmlRoot("item")]
public class VersionInfoDto
{
    [XmlElement("version")]
    public string Version { get; set; } = string.Empty;

    [XmlElement("url")]
    public string Url { get; set; } = string.Empty;

    [XmlElement("changelog")]
    public string Changelog { get; set; } = string.Empty;

    [XmlElement("mandatory")]
    public bool Mandatory { get; set; }
}
