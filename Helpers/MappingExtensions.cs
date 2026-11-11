using BookingSystemAPI.DTOs;
using BookingSystemAPI.Models;

namespace BookingSystemAPI.Helpers
{
    public static class MappingExtensions
    {
        // SQL Server returns dates without a time zone marker. This tells the JSON
        // serializer the value is UTC, so Angular converts it to local time correctly.
        public static DateTime AsUtc(this DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

        public static DateTime? AsUtc(this DateTime? value) =>
            value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

        // ---------- Parent ----------
        public static ParentResponseDto ToDto(this Parent parent) => new()
        {
            ParentId = parent.ParentId,
            FirstName = parent.FirstName,
            LastName = parent.LastName,
            Email = parent.Email,
            PhoneNumber = parent.PhoneNumber
        };

        public static ParentDetailsDto ToDetailsDto(this Parent parent) => new()
        {
            ParentId = parent.ParentId,
            FirstName = parent.FirstName,
            LastName = parent.LastName,
            Email = parent.Email,
            PhoneNumber = parent.PhoneNumber,
            Students = parent.Students
                .OrderBy(s => s.FirstName)
                .Select(s => s.ToDtoWithBookings())
                .ToList()
        };

        // ---------- Student ----------
        public static StudentResponseDto ToDto(this Student student) => new()
        {
            StudentNo = student.StudentNo,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Grade = student.Grade,
            HomeLanguage = student.HomeLanguage,
            ParentId = student.ParentId,
            ParentName = student.Parent == null ? string.Empty : $"{student.Parent.FirstName} {student.Parent.LastName}"
        };

        public static StudentWithBookingsDto ToDtoWithBookings(this Student student) => new()
        {
            StudentNo = student.StudentNo,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Grade = student.Grade,
            HomeLanguage = student.HomeLanguage,
            ParentId = student.ParentId,
            ParentName = student.Parent == null ? string.Empty : $"{student.Parent.FirstName} {student.Parent.LastName}",
            Bookings = student.Bookings
                .OrderByDescending(b => b.DateMade)
                .Select(b => b.ToDto())
                .ToList()
        };

        // ---------- Booking (needs Student and Slot loaded) ----------
        public static BookingResponseDto ToDto(this Booking booking) => new()
        {
            BookingId = booking.BookingId,
            BookingNumber = booking.BookingNumber,
            Status = booking.Status,
            StudentNo = booking.StudentNo,
            StudentName = booking.Student == null ? string.Empty : $"{booking.Student.FirstName} {booking.Student.LastName}",
            SlotId = booking.SlotId,
            AssessmentDate = booking.Slot?.AssessmentDate ?? default,
            StartTime = booking.Slot?.StartTime ?? default,
            EndTime = booking.Slot?.EndTime ?? default,
            DateMade = booking.DateMade.AsUtc()
        };

        // Needs Student, Parent, Slot loaded.
        public static BookingDetailsDto ToDetailsDto(this Booking booking) => new()
        {
            BookingId = booking.BookingId,
            BookingNumber = booking.BookingNumber,
            Status = booking.Status,
            StudentNo = booking.StudentNo,
            StudentName = booking.Student == null ? string.Empty : $"{booking.Student.FirstName} {booking.Student.LastName}",
            SlotId = booking.SlotId,
            AssessmentDate = booking.Slot?.AssessmentDate ?? default,
            StartTime = booking.Slot?.StartTime ?? default,
            EndTime = booking.Slot?.EndTime ?? default,
            DateMade = booking.DateMade.AsUtc(),

            ParentId = booking.ParentId,
            ParentName = booking.Parent == null ? string.Empty : $"{booking.Parent.FirstName} {booking.Parent.LastName}",
            ParentEmail = booking.Parent?.Email ?? string.Empty,
            ParentPhoneNumber = booking.Parent?.PhoneNumber ?? string.Empty,
            StudentGrade = booking.Student?.Grade ?? string.Empty,
            StudentHomeLanguage = booking.Student?.HomeLanguage ?? string.Empty,
            LastModifiedAt = booking.LastModifiedAt.AsUtc(),
            CancelledAt = booking.CancelledAt.AsUtc(),
            CancellationReason = booking.CancellationReason
        };

        // ---------- Staff (needs Role loaded) ----------
        public static StaffResponseDto ToDto(this Staff staff) => new()
        {
            StaffNo = staff.StaffNo,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber,
            RoleName = staff.Role?.RoleName ?? string.Empty,
            IsActive = staff.IsActive
        };
    }
}