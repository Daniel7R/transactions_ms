namespace PaymentsMS.Application.Messages
{
    public class GenerateTicketSale
    {
        public int IdTransaction { get; set; }
        public int IdUser { get; set; }
        public int IdTicket { get; set; }
    }
}
