using Microsoft.EntityFrameworkCore;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
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
                entity.Property(e => e.CustomerFullName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Telephone).HasMaxLength(12).IsRequired();
                entity.Property(e => e.EmailAddress).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CustomerBirthday).HasColumnType("datetime2");
                entity.HasIndex(e => e.EmailAddress).IsUnique();
            });

            // Configure RoomType entity
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasKey(e => e.RoomTypeID);
                entity.Property(e => e.RoomTypeName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TypeDescription).HasMaxLength(250);
                entity.Property(e => e.TypeNote).HasMaxLength(250);
            });

            // Configure RoomInformation entity
            modelBuilder.Entity<RoomInformation>(entity =>
            {
                entity.HasKey(e => e.RoomID);
                entity.Property(e => e.RoomNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.RoomDescription).HasMaxLength(220);
                entity.Property(e => e.RoomPricePerDate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RoomStatus).HasColumnType("int");
                entity.HasIndex(e => e.RoomNumber).IsUnique();
                
                entity.HasOne(e => e.RoomType)
                      .WithMany()
                      .HasForeignKey(e => e.RoomTypeID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingID);
                entity.Property(e => e.CheckInDate).HasColumnType("datetime2");
                entity.Property(e => e.CheckOutDate).HasColumnType("datetime2");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime2");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
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
