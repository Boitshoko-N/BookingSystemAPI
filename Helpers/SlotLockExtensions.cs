using BookingSystemAPI.Data;
using BookingSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Helpers
{
    public static class SlotLockExtensions
    {
        // Loads the slot and locks its row until the surrounding transaction commits or rolls back.
        // Returns null if the slot does not exist.
        // Call this BEFORE loading anything else about the slot, so you never work with stale data.
        public static async Task<Slot?> LockSlotAsync(this ApplicationDbContext context, int slotId)
        {
            var slots = await context.Slots
                .FromSqlInterpolated($"SELECT * FROM Slots WITH (UPDLOCK, ROWLOCK) WHERE SlotId = {slotId}")
                .ToListAsync();

            return slots.SingleOrDefault();
        }
    }
}