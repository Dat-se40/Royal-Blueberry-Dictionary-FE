using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Word;

namespace Royal_Blueberry_Dictionary.Repository.Interface
{
    public interface ITagRepository
    {
        // Tag Operations
        Task<Tag?> GetTagByIdAsync(string id);
        Task<List<Tag>> GetAllTagsAsync(string userId);
        Task<List<Tag>> GetDirtyTagsAsync(string userId);
        Task AddTagAsync(Tag tag);
        Task UpdateTagAsync(Tag tag);
        Task DeleteTagAsync(string id);

        // Relation Operations (Class Meta)
        Task<List<WordTagRelation>> GetRelationsByTagAsync(string tagId);
        Task<List<WordTagRelation>> GetRelationsByWordAsync(string userId, string word, int meaningIndex);
        Task<List<WordTagRelation>> GetDirtyRelationsAsync(string userId);
        Task AddRelationAsync(WordTagRelation relation);
        Task RemoveRelationAsync(string tagId, string word, int meaningIndex);

        Task SaveChangesAsync();
    }
}