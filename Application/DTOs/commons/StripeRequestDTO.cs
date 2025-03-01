namespace PaymentsMS.Application.DTOs.commons
{
    /// <summary>
    ///  This dto is used to create a payment, transaction, donation, etc.
    /// </summary>
    public abstract class StripeRequestDTO
    {
        /// <summary>
        /// Url for stripe session
        /// </summary>
        public string SessionUrl { get; set; }
        /// <summary>
        /// This attribute is used to create a stripe session
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// Approved url when a transaction was successfull
        /// </summary>
        public string ApprovedUrl { get; set; }
        /// <summary>
        /// Cancel url when a transaction is cancelled or rejected
        /// </summary>
        public string CancelUrl { get; set; }
    }
}
