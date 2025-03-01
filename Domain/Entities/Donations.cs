using System.ComponentModel.DataAnnotations;

namespace PaymentsMS.Domain.Entities
{
    public class Donations
    {
        public int Id { get; set; }
        [Required]
        public int IdUser { get; set; }
        [Required]
        public int IdTournament {  get; set; }
        [Required]
        public int IdTransaction {  get; set; }
        public Transactions Transaction { get; set; }
        //[Required]
        //public decimal Total {  get; set; }
    }
}
