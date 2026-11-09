using Microsoft.EntityFrameworkCore;
using BookingSystemAPI.Models;

namespace BookingSystemAPI.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>()
                .HasMany(p => p.Students)
                .WithOne(s => s.Parent);
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Parent)
                .WithMany(p => p.Students)
                .HasForeignKey(s => s.ParentId);
            modelBuilder.Entity<Parent>()
                .HasMany(p => p.Bookings)
                .WithOne(b => b.Parent);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Parent)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.ParentId);
            modelBuilder.Entity<Slot>()
                .HasMany(s => s.Bookings)
                .WithOne(b => b.Slot);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Slot)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SlotId);
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Role)
                .WithMany(r => r.StaffMembers)
                .HasForeignKey(s => s.RoleId);







        }

    }
}
