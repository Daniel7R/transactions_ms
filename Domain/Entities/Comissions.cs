﻿using System.ComponentModel.DataAnnotations;

namespace PaymentsMS.Domain.Entities
{
    public class Comissions
    {
        public int Id { get; set; }
        [Required]
        public int IdTransaction { get; set; }
        public Transactions Transaction { get; set; }
        [Required]
        public decimal Percent { get; set; }
        [Required]  
        public decimal Total {  get; set; }
    }
}
