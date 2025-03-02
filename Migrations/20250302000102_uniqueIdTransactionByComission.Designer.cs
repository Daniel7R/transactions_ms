﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PaymentsMS.Infrastructure.Data;

#nullable disable

namespace PaymentsMS.Migrations
{
    [DbContext(typeof(TransactionsDbContext))]
    [Migration("20250302000102_uniqueIdTransactionByComission")]
    partial class uniqueIdTransactionByComission
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PaymentsMS.Domain.Entities.Comissions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("IdTransaction")
                        .HasColumnType("integer")
                        .HasColumnName("id_transaction");

                    b.Property<decimal>("Percent")
                        .HasColumnType("numeric")
                        .HasColumnName("percent");

                    b.Property<decimal>("Total")
                        .HasColumnType("numeric")
                        .HasColumnName("total");

                    b.HasKey("Id");

                    b.HasIndex("IdTransaction")
                        .IsUnique();

                    b.ToTable("comissions", (string)null);
                });

            modelBuilder.Entity("PaymentsMS.Domain.Entities.Donations", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("IdTournament")
                        .HasColumnType("integer")
                        .HasColumnName("id_tournament");

                    b.Property<int>("IdTransaction")
                        .HasColumnType("integer")
                        .HasColumnName("id_transaction");

                    b.Property<int>("IdUser")
                        .HasColumnType("integer")
                        .HasColumnName("id_user");

                    b.HasKey("Id");

                    b.HasIndex("IdTransaction")
                        .IsUnique();

                    b.ToTable("donations", (string)null);
                });

            modelBuilder.Entity("PaymentsMS.Domain.Entities.Transactions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("IdUser")
                        .HasColumnType("integer")
                        .HasColumnName("id_name");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("numeric")
                        .HasColumnName("quantity");

                    b.Property<string>("StripeSessionId")
                        .HasColumnType("text")
                        .HasColumnName("stripe_session_id");

                    b.Property<DateTime?>("TransactionCompletedDate")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("transaction_completed_date");

                    b.Property<DateTime>("TransactionStartedDate")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("transaction_start_date");

                    b.Property<string>("TransactionStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("transaction_status");

                    b.Property<string>("TransactionType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("transaction_type");

                    b.HasKey("Id");

                    b.ToTable("transactions", (string)null);
                });

            modelBuilder.Entity("PaymentsMS.Domain.Entities.Comissions", b =>
                {
                    b.HasOne("PaymentsMS.Domain.Entities.Transactions", "Transaction")
                        .WithOne("Comission")
                        .HasForeignKey("PaymentsMS.Domain.Entities.Comissions", "IdTransaction")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("PaymentsMS.Domain.Entities.Donations", b =>
                {
                    b.HasOne("PaymentsMS.Domain.Entities.Transactions", "Transaction")
                        .WithOne("Donation")
                        .HasForeignKey("PaymentsMS.Domain.Entities.Donations", "IdTransaction")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("PaymentsMS.Domain.Entities.Transactions", b =>
                {
                    b.Navigation("Comission")
                        .IsRequired();

                    b.Navigation("Donation")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
