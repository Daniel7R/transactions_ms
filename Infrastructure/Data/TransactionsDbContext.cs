﻿using Microsoft.EntityFrameworkCore;
using PaymentsMS.Domain.Entities;

namespace PaymentsMS.Infrastructure.Data
{
    public class TransactionsDbContext : DbContext
    {
        DbSet<Transactions> Transactions { get; set; }
        DbSet<Comissions> Comissions { get; set; }
        DbSet<Donations> Donations { get; set; }

        public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transactions>().ToTable("transactions");
            modelBuilder.Entity<Donations>().ToTable("donations");
            modelBuilder.Entity<Comissions>().ToTable("comissions");

            modelBuilder.Entity<Transactions>()
                .Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Transactions>()
                .Property(t => t.StripeSessionId)
                .HasColumnName("stripe_session_id");
            modelBuilder.Entity<Transactions>()
                .Property(t => t.Quantity)
                .HasColumnName("quantity");
            modelBuilder.Entity<Transactions>()
                .Property(t => t.TransactionType)
                .HasColumnName("transaction_type")
                .HasConversion<string>();
            modelBuilder.Entity<Transactions>()
                .Property(t => t.TransactionStartedDate)
                .HasColumnName("transaction_start_date");
            modelBuilder.Entity<Transactions>()
                .Property(t => t.TransactionCompletedDate)
                .HasColumnName("transaction_completed_date");
            modelBuilder.Entity<Transactions>()
                .Property(t => t.TransactionStatus)
                .HasColumnName("transaction_status")
                .HasConversion<string>();
            modelBuilder.Entity<Transactions>()
                .Property(t => t.IdUser)
                .HasColumnName("id_name");

            modelBuilder.Entity<Comissions>()
                .Property(c => c.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Comissions>()
                .Property(c => c.IdTransaction)
                .HasColumnName("id_transaction");
            modelBuilder.Entity<Comissions>()
                .Property(c => c.Percent)
                .HasColumnName("percent");
            modelBuilder.Entity<Comissions>()
                .Property(c => c.Total)
                .HasColumnName("total");

            modelBuilder.Entity<Comissions>()
                .HasIndex(c => c.IdTransaction)
                .IsUnique();

            modelBuilder.Entity<Donations>()
                .Property(d => d.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Donations>()
                .Property(d => d.IdTransaction)
                .HasColumnName("id_transaction");
            modelBuilder.Entity<Donations>()
                .Property(d => d.IdTournament)
                .HasColumnName("id_tournament");
            modelBuilder.Entity<Donations>()
                .Property(d => d.IdUser)
                .HasColumnName("id_user");
            //modelBuilder.Entity<Donations>()
              //  .Property(d => d.Total)
                //.HasColumnName("total");

            modelBuilder.Entity<Comissions>()
                .HasOne(c => c.Transaction)
                .WithOne(t => t.Comission)
                .HasForeignKey<Comissions>(c => c.IdTransaction);

            modelBuilder.Entity<Donations>()
                .HasOne(d => d.Transaction)
                .WithOne(t => t.Donation)
                .HasForeignKey<Donations>(d => d.IdTransaction);
        
        }
    }
}
