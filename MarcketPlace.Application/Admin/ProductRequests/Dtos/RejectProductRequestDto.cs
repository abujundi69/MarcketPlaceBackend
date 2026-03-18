using System.Text.Json.Serialization;

namespace MarcketPlace.Application.Admin.ProductRequests.Dtos
{
    public class RejectProductRequestDto
    {
        [JsonPropertyName("note")]
        public string? AdminNote { get; set; }
    }
}
