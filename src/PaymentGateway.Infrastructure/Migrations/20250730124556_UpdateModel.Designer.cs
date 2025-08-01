using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PaymentGateway.Infrastructure.Data;

#nullable disable

namespace PaymentGateway.Infrastructure.Migrations
{
    [DbContext(typeof(PaymentDbContext))]
    [Migration("20250730124556_UpdateModel")]
    partial class UpdateModel
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PaymentGateway.Core.Entities.OutboxEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Error")
                        .HasColumnType("text");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("OccurredAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Processed")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("ProcessedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Processing")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("ProcessingAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("OutboxEvents");
                });

            modelBuilder.Entity("PaymentGateway.Core.Entities.Payment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("Amount")
                        .HasColumnType("bigint");

                    b.Property<string>("AuthorizationCode")
                        .HasColumnType("text");

                    b.Property<string>("CardLastFour")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ExpiryMonth")
                        .HasColumnType("integer");

                    b.Property<int>("ExpiryYear")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Payments");
                });
        }
    }
}
