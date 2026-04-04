using Microsoft.EntityFrameworkCore;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public async Task AddAsync(WordEntry entry)
        {
            appDbContext.Add(entry);
            await appDbContext.SaveChangesAsync();
        }

        public Task DeleteAsync(string id)
        {
            appDbContext.Remove(new WordEntry { Id = id });
            return appDbContext.SaveChangesAsync(); 
        }

        public Task<List<WordEntry>> GetAllAsync(string userId)
        {
            return appDbContext.WordEntries.Where(e => e.UserId == userId).ToListAsync();  
        }

        public async Task<WordEntry> GetByIdAsync(string id)
        {
            var result = await appDbContext.WordEntries.FirstOrDefaultAsync(e => e.Id == id);
            return result;
        }

        public Task<List<WordEntry>> GetByTagAsync(string tagId)
        {
            //var result = appDbContext.WordEntries.Where(e => e.TagIdsJson.Contains(tagId)).ToListAsync();
            //return result;  
            throw new NotImplementedException();
        }

        public Task<WordEntry> GetByWordAndMeaningAsync(string userId, string word, int meaningIndex)
        {
            throw new NotImplementedException();
        }

        public async Task<List<WordEntry>> GetDirtyAsync(string userId)
        {
            var result = await appDbContext.WordEntries.Where(e => e.UserId == userId && e.IsDirty).ToListAsync();
            return result;
        }

        public async Task<List<WordEntry>> GetAllWordEntriesAsync()
        {
            var result = await appDbContext.WordEntries.ToListAsync();
            return result;
        }
        public async Task<List<WordEntry>> GetPartOfSpeech(string part) 
        {
            var result = await appDbContext.WordEntries.Where(e => e.PartOfSpeech == part).ToListAsync();   
            return result;  
        }
        public async Task<List<WordEntry>> GetByLetter(char letter)
        {
            var result = await appDbContext.WordEntries.Where(e => e.Word.StartsWith(letter)).ToListAsync();
            return result;
        }
        public async Task<List<WordEntry>> GeAlltFavorited() 
            var result = await appDbContext.WordEntries.Where(e => e.IsFavorited).ToListAsync();    
        {
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
