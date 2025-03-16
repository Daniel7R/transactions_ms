using PaymentsMS.Application.Messages.Enums;

namespace PaymentsMS.Application.Messages
{
    public class GetTicketInfoResponse
    {
        public int IdTicket { get; set; }
        public int? IdTournament { get; set; }
        public TicketType Type { get; set; }
        public TicketStatus Status { get; set; }
    }
}
