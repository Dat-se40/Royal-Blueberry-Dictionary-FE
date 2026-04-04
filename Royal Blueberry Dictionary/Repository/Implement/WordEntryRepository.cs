using Microsoft.EntityFrameworkCore;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;

namespace Royal_Blueberry_Dictionary.Repository.Implement
{
    public class WordEntryRepository : IWordEntryRepository
    {
        private readonly AppDbContext _context;

        public WordEntryRepository(AppDbContext context)
        {
            _context = context;
        }

        private string GetEffectiveId(string userId) => string.IsNullOrEmpty(userId) ? "GUEST" : userId;

        public async Task<WordEntry?> GetByIdAsync(string id) =>
            await _context.WordEntries.FirstOrDefaultAsync(e => e.Id == id);

        public async Task<List<WordEntry>> GetAllAsync(string userId) =>
            await _context.WordEntries.Where(e => e.UserId == GetEffectiveId(userId)).ToListAsync();

        public async Task<WordEntry?> GetByWordAndMeaningAsync(string userId, string word, int meaningIndex) =>
            await _context.WordEntries.FirstOrDefaultAsync(e =>
                e.UserId == GetEffectiveId(userId) &&
                e.Word.ToLower() == word.ToLower() &&
                e.MeaningIndex == meaningIndex);

        public async Task<List<WordEntry>> GetDirtyAsync(string userId) =>
            await _context.WordEntries.Where(e => e.UserId == GetEffectiveId(userId) && e.IsDirty).ToListAsync();

        public async Task<List<WordEntry>> GetFavoritedAsync(string userId) =>
            await _context.WordEntries.Where(e => e.UserId == GetEffectiveId(userId) && e.IsFavorited).ToListAsync();

        public async Task AddAsync(WordEntry entry)
        {
            entry.UserId = GetEffectiveId(entry.UserId);
            entry.IsDirty = true;
            await _context.WordEntries.AddAsync(entry);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WordEntry entry)
        {
            entry.IsDirty = true;
            entry.LastModifiedAt = DateTime.UtcNow;
            _context.WordEntries.Update(entry);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entry = await GetByIdAsync(id);
            if (entry != null)
            {
                _context.WordEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
