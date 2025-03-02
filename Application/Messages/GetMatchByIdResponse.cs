namespace PaymentsMS.Application.Messages
{
    public class GetMatchByIdResponse
    {
        public int IdMatch { get; set; }
        public string Name {  get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}
