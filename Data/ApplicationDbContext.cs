using BookingSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<AssessmentResult> AssessmentResults { get; set; }
        public DbSet<AssessmentCriteria> CriteriaSettings { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Role ----------
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.RoleName).IsUnique();

                entity.HasData(
                    new Role { RoleId = 1, RoleName = RoleNames.Admin, RoleDescription = "Full access, including staff accounts and pass criteria." },
                    new Role { RoleId = 2, RoleName = RoleNames.Clerk, RoleDescription = "Manages slots, bookings and assessment results." });
            });

            // ---------- Staff ----------
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasIndex(s => s.Email).IsUnique();

                entity.HasOne(s => s.Role)
                    .WithMany(r => r.StaffMembers)
                    .HasForeignKey(s => s.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Parent ----------
            modelBuilder.Entity<Parent>(entity =>
            {
                entity.HasIndex(p => p.Email).IsUnique();
            });

            // ---------- Student ----------
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasOne(s => s.Parent)
                    .WithMany(p => p.Students)
                    .HasForeignKey(s => s.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Slot ----------
            modelBuilder.Entity<Slot>(entity =>
            {
                entity.Property(s => s.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasIndex(s => s.AssessmentDate);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Slots_MaxBookings", "[MaxBookings] > 0");
                    t.HasCheckConstraint("CK_Slots_EndAfterStart", "[EndTime] > [StartTime]");
                });
            });

            // ---------- Booking ----------
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(b => b.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // Final guarantee that booking numbers are unique.
                entity.HasIndex(b => b.BookingNumber).IsUnique();

                // A student can have only ONE Confirmed booking at a time.
                entity.HasIndex(b => b.StudentNo)
                    .IsUnique()
                    .HasFilter("[Status] = 'Confirmed'");

                entity.HasOne(b => b.Parent)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(b => b.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Student)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(b => b.StudentNo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Slot)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(b => b.SlotId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- AssessmentResult ----------
            modelBuilder.Entity<AssessmentResult>(entity =>
            {
                entity.Property(r => r.Outcome)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // One-to-one: a booking has at most one result (also creates a unique index).
                // One-to-one: a student has at most one result (also creates a unique index).
                entity.HasOne(r => r.Student)
                    .WithOne(s => s.AssessmentResult)
                    .HasForeignKey<AssessmentResult>(r => r.StudentNo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.CapturedBy)
                    .WithMany()
                    .HasForeignKey(r => r.CapturedByStaffNo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_AssessmentResults_Mathematics", "[MathematicsMark] IS NULL OR [MathematicsMark] BETWEEN 0 AND 100");
                    t.HasCheckConstraint("CK_AssessmentResults_English", "[EnglishMark] IS NULL OR [EnglishMark] BETWEEN 0 AND 100");
                    t.HasCheckConstraint("CK_AssessmentResults_HomeLanguage", "[HomeLanguageMark] IS NULL OR [HomeLanguageMark] BETWEEN 0 AND 100");
                });
            });

            // ---------- AssessmentCriteria ----------
            modelBuilder.Entity<AssessmentCriteria>(entity =>
            {
                entity.HasOne<Staff>()
                    .WithMany()
                    .HasForeignKey(c => c.UpdatedByStaffNo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- EmailLog ----------
            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.Property(e => e.Type)
                    .HasConversion<string>()
                    .HasMaxLength(40);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasIndex(e => e.Status);

                entity.HasOne<Booking>()
                    .WithMany()
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Subject (kept for later expansion) ----------
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasOne(s => s.Staff)
                    .WithMany()
                    .HasForeignKey(s => s.StaffNo)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}