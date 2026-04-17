using System.Text.Json.Serialization;

namespace OresToFieldGuide
{
	public class Tag
	{
		[JsonPropertyName("values")]
		public required List<string> Values { get; set; }
	}
}
