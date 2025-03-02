using System.Text.Json.Serialization;

namespace PaymentsMS.Application.DTOs.commons
{
    public class SaleParticipantDetailsDTO
    {
        public int IdTicket { get; set; }
        public int IdTournament { get; set; }
        [JsonIgnore]
        public bool IsFree {get; set;}
        //public decimal Price { get; set; }
    }
}
