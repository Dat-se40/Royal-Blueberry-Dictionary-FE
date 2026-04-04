using Royal_Blueberry_Dictionary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Repository.Interface
{
    public interface IWordEntryRepository
    {
        Task<WordEntry> GetByIdAsync(string id);
        Task<List<WordEntry>> GetAllAsync(string userId);
        Task<List<WordEntry>> GetByTagAsync(string tagId);
        Task<List<WordEntry>> GetDirtyAsync(string userId);
        Task<WordEntry> GetByWordAndMeaningAsync(string userId, string word, int meaningIndex);

        Task<List<WordEntry>> GetAllWordEntriesAsync();

        Task AddAsync(WordEntry entry);
        Task UpdateAsync(WordEntry entry);
        Task DeleteAsync(string id);
        Task SaveChangesAsync();
    }
}
