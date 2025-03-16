namespace PaymentsMS.Application.Messages
{
    public class EmailNotificationRequest
    {
        /// <summary>
        /// With id can get email
        /// </summary>
        public int IdUser { get; set; }
        /// <summary>
        ///  Email subject
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        ///  Body of the email
        /// </summary>
        public string Body { get; set; }
    }
}
