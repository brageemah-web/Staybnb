using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using staybnb.Models;

namespace staybnb.Data

{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<Amenity> Amenities => Set<Amenity>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<CheckInProcess> CheckInProcesses => Set<CheckInProcess>();
        public DbSet<GuestCheckIn> GuestCheckIns => Set<GuestCheckIn>();
        public  DbSet<GuestDocument> GuestDocuments => Set<GuestDocument>();
        public DbSet<HostApplication> HostApplications => Set<HostApplication>();
        public DbSet<HostProperty> HostProperties => Set<HostProperty>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<WishListItem> WishListItems => Set<WishListItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HostProperty>()
                .HasOne(p => p.Host).WithMany(u => u.Properties)
                .HasForeignKey(p => p.HostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropertyImage>()
                .HasOne(i => i.Property).WithMany(p => p.Images)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HostApplication>()
                .HasOne(a => a.ApplicationUser).WithMany()
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HostApplication>()
                .HasOne(a => a.Property).WithMany()
                .HasForeignKey(a => a.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(p => p.Property).WithMany(b => b.Bookings)
                .HasForeignKey(p => p.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(g => g.Guest).WithMany(b => b.Bookings)
                .HasForeignKey(g => g.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment).WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
            .HasOne(x => x.GuestCheckIn).WithOne(g => g.Booking)
            .HasForeignKey<GuestCheckIn>(g => g.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<HostProperty>()
                .HasOne(p => p.CheckInProcess).WithOne(c => c.Property)
                .HasForeignKey<CheckInProcess>(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuestCheckIn>()
                .HasOne(g => g.CheckInProcess).WithMany()
                .HasForeignKey(g => g.CheckInProcessId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GuestDocument>()
                .HasOne(d => d.GuestCheckIn).WithMany(g => g.Documents)
                .HasForeignKey(d => d.GuestCheckInId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Property).WithMany(p => p.Reviews)
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer).WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WishListItem>()
                .HasOne(w => w.User).WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WishListItem>()
                .HasOne(w => w.Property).WithMany()
                .HasForeignKey(w => w.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender).WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver).WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User).WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityLog>()
                .HasOne(a => a.User).WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Amenity>().HasData(
                new Amenity { Id = 1, Name = "WiFi", IconClass = "bi-wifi" },
                new Amenity { Id = 2, Name = "Kitchen", IconClass = "bi-egg-fried" },
                new Amenity { Id = 3, Name = "Washer", IconClass = "bi-droplet" },
                new Amenity { Id = 4, Name = "Air conditioning", IconClass = "bi-snow" },
                new Amenity { Id = 5, Name = "Heating", IconClass = "bi-thermometer-sun" },
                new Amenity { Id = 6, Name = "Parking", IconClass = "bi-car-front" },
                new Amenity { Id = 7, Name = "Pool", IconClass = "bi-water" },
                new Amenity { Id = 8, Name = "TV", IconClass = "bi-tv" }
            );
        }
    }
 }
 
