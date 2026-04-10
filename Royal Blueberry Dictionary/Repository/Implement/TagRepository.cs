using Microsoft.EntityFrameworkCore;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Word;
using Royal_Blueberry_Dictionary.Repository.Interface;

namespace Royal_Blueberry_Dictionary.Repository.Implement
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _context;

        public TagRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tag?> GetTagByIdAsync(string id) => await _context.Tags.FindAsync(id);

        public async Task<List<Tag>> GetAllTagsAsync() =>
            await _context.Tags.Where(t => t.UserId == App.UserId).ToListAsync();

        public async Task<List<Tag>> GetDirtyTagsAsync() =>
            await _context.Tags.Where(t => t.UserId == App.UserId && t.IsDirty).ToListAsync();

        public async Task AddTagAsync(Tag tag)
        {
            tag.UserId = App.UserId;
            tag.IsDirty = true;
            await _context.Tags.AddAsync(tag);
        }

        public async Task UpdateTagAsync(Tag tag)
        {
            tag.IsDirty = true;
            tag.LastModifiedAt = DateTime.UtcNow;
            _context.Tags.Update(tag);
        }

        public async Task DeleteTagAsync(string id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null) _context.Tags.Remove(tag);
        }
        public async Task<List<WordTagRelation>> GetRelationsByTagAsync(string tagId) =>
            await _context.WordTagRelations.Where(r => r.TagId == tagId).ToListAsync();

        public async Task<List<WordTagRelation>> GetRelationsByWordAsync(string userId, string word, int meaningIndex) =>
            await _context.WordTagRelations.Where(r =>
                r.UserId == App.UserId &&
                r.Word.ToLower() == word.ToLower() &&
                r.MeaningIndex == meaningIndex).ToListAsync();

        public async Task<List<WordTagRelation>> GetDirtyRelationsAsync(string userId) =>
            await _context.WordTagRelations.Where(r => r.UserId == App.UserId && r.IsDirty).ToListAsync();

        public async Task AddRelationAsync(WordTagRelation relation)
        {
            relation.UserId = App.UserId;
            relation.IsDirty = true;
            // Kiểm tra trùng lặp trước khi add
            var exists = await _context.WordTagRelations.AnyAsync(r =>
                r.TagId == relation.TagId && r.Word == relation.Word && r.MeaningIndex == relation.MeaningIndex);

            if (!exists) await _context.WordTagRelations.AddAsync(relation);
        }

        public async Task RemoveRelationAsync(string tagId, string word, int meaningIndex)
        {
            var rel = await _context.WordTagRelations.FirstOrDefaultAsync(r =>
                r.TagId == tagId && r.Word == word && r.MeaningIndex == meaningIndex);
            if (rel != null) _context.WordTagRelations.Remove(rel);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}