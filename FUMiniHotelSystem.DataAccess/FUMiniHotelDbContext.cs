using Microsoft.EntityFrameworkCore;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public class FUMiniHotelDbContext : DbContext
    {
        public FUMiniHotelDbContext(DbContextOptions<FUMiniHotelDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<RoomInformation> RoomInformation { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerID);
                entity.Property(e => e.CustomerFullName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Telephone).IsRequired().HasMaxLength(12);
                entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerBirthday).IsRequired();
                entity.Property(e => e.CustomerStatus).IsRequired();
                entity.Property(e => e.Password).IsRequired().HasMaxLength(50);
            });

            // Configure RoomType entity
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasKey(e => e.RoomTypeID);
                entity.Property(e => e.RoomTypeName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TypeDescription).HasMaxLength(250);
                entity.Property(e => e.TypeNote).HasMaxLength(250);
            });

            // Configure RoomInformation entity
            modelBuilder.Entity<RoomInformation>(entity =>
            {
                entity.HasKey(e => e.RoomID);
                entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RoomDescription).HasMaxLength(220);
                entity.Property(e => e.RoomMaxCapacity).IsRequired();
                entity.Property(e => e.RoomStatus).IsRequired();
                entity.Property(e => e.RoomPricePerDate).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.RoomTypeID).IsRequired();

                entity.HasOne(e => e.RoomType)
                    .WithMany()
                    .HasForeignKey(e => e.RoomTypeID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingID);
                entity.Property(e => e.CustomerID).IsRequired();
                entity.Property(e => e.RoomID).IsRequired();
                entity.Property(e => e.CheckInDate).IsRequired();
                entity.Property(e => e.CheckOutDate).IsRequired();
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.BookingStatus).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RoomInformation)
                    .WithMany()
                    .HasForeignKey(e => e.RoomID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
