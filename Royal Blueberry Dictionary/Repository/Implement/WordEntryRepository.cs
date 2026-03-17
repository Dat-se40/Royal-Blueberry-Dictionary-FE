using Microsoft.EntityFrameworkCore;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Repository.Implement
{
    public class WordEntryRepository : IWordEntryRepository
    {
        private readonly AppDbContext appDbContext;

        public WordEntryRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        public Task AddAsync(WordEntry entry)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<WordEntry>> GetAllAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<WordEntry> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<WordEntry>> GetByTagAsync(string userId, string tagId)
        {
            throw new NotImplementedException();
        }

        public Task<WordEntry> GetByWordAndMeaningAsync(string userId, string word, int meaningIndex)
        {
            throw new NotImplementedException();
        }

        public Task<List<WordEntry>> GetDirtyAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<WordEntry>> GetAllWordEntriesAsync()
        {
            var result = await appDbContext.WordEntries.ToListAsync();
            return result;
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(WordEntry entry)
        {
            throw new NotImplementedException();
        }
    }
}
