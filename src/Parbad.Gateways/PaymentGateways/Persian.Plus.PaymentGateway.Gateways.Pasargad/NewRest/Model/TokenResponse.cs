using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Persian.Plus.PaymentGateway.Gateways.Pasargad.NewRest.Model
{
    public class TokenResponse
    {
        [JsonProperty("resultMsg")]
        [JsonPropertyName("resultMsg")]
        public string ResultMsg { get; set; }

        [JsonProperty("resultCode")]
        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty("token")]
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonProperty("username")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonProperty("firstName")]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonProperty("userId")]
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonProperty("roles")]
        [JsonPropertyName("roles")]
        public List<Role> Roles { get; set; }
    }


}