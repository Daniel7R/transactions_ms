namespace PaymentsMS.Application.DTOs.commons
{
    public class CacheInfoSaleDTO
    {
        public int IdUser { get; set; }
        /// <summary>
        /// If ticket id is null, it is viewer ticket and must be created through QueueManager
        /// </summary>
        public int? IdTicket { get; set; } = null;
        /// <summary>
        /// Match id is used for sale
        /// </summary>
        public int? IdMatch { get; set; } = null;
    }
}
