    using BlueBerryDictionary.Helpers;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service
{
    public class WordService
    {
        IWordEntryRepository WordEntryRepository; 
        public WordService(IWordEntryRepository wordEntryRepository )
        {
            WordEntryRepository = wordEntryRepository;        
        }
        public async Task<WordEntry> GetWordEntryByDetail(WordDetail detail, int meaningIdx, int defIdx) 
        {
            var newEntry  = await WordEntryRepository.GetByWordAndMeaningAsync(App.UserId,detail.Word, meaningIdx); 
            
            try
            {
                if (newEntry == null) 
                {
                    newEntry = MapWordDetailToWordEntry(detail, meaningIdx, defIdx);
                    await SmartUpdate(newEntry); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            return newEntry;
        }

        public async Task<List<WordEntry>> GetAllWordsAsync()
        {
            return await WordEntryRepository.GetAllAsync(App.UserId);
        }
        public async Task<WordEntry> GetWordEntryByID (string ID) 
        {
            var res = await WordEntryRepository.GetByIdAsync(ID);
            return res;
        }
        public async void DeletedEntry(string ID)
        {
            await WordEntryRepository.DeleteAsync(ID); 
        }

        public Task DeleteWordEntryAsync(string id) => WordEntryRepository.DeleteAsync(id);
        public static WordEntry? MapWordDetailToWordEntry(WordDetail detail, int meaningIdx, int defIdx)
        {
          
            try
            {
                var meaning = detail.Meanings[meaningIdx];
                var definition = new Definition();
                definition = meaning.Definitions[defIdx];

                return new WordEntry
                {
                    Word = detail.Word ?? string.Empty,
                    Phonetic = detail.Phonetic ?? string.Empty,
                    PartOfSpeech = meaning.PartOfSpeech ?? string.Empty,
                    Definition = definition.Text ?? string.Empty,
                    Example = definition.Example ?? string.Empty,
                    Note = string.Empty,

                    MeaningIndex = meaningIdx,
                    LastModifiedAt = DateTime.UtcNow,
                    IsDirty = true
                };
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return null; 
            }

        }
        public async Task FavoriteAsync(WordEntry wordEntry)
        {
            wordEntry.IsFavorited = !wordEntry.IsFavorited;
            var ts = App.serviceProvider.GetRequiredService<ITagRepository>();
            var ls = await ts.GetRelationsByWordAsync(App.UserId, wordEntry.Word, wordEntry.MeaningIndex);
            foreach (var item in ls)
            {
                item.IsFavourite = wordEntry.IsFavorited;  
                item.Note = wordEntry.Note; 
            }
            await Task.WhenAll(ts.SaveChangesAsync(),SmartUpdate(wordEntry));  
        }

        /// <summary>Danh sách từ đã gắn cờ yêu thích trong DB (theo user).</summary>
        public async Task<List<WordEntry>> GetFavoritedWordsAsync() =>
            await WordEntryRepository.GetFavoritedAsync(App.UserId);

        /// <summary>Bỏ yêu thích toàn bộ từ (không dùng toggle của <see cref="FavoriteAsync"/>).</summary>
        public async Task ClearAllFavoritesAsync()
        {
            var list = await WordEntryRepository.GetFavoritedAsync(App.UserId);
            foreach (var e in list)
            {
                e.IsFavorited = false;
                await SmartUpdate(e);
            }
        }
        public async Task SmartUpdate(WordEntry wordEntry) 
        {
            var existing = await WordEntryRepository.GetByIdAsync(wordEntry.Id);

            if (existing == null)
            {
                await WordEntryRepository.AddAsync(wordEntry);
            }
            else
            {
                await WordEntryRepository.UpdateAsync(wordEntry);
            }
        }
        public async Task CleanUpData()
        {
            var list = await WordEntryRepository.GetAllAsync(App.UserId);
            var db = App.serviceProvider.GetRequiredService<AppDbContext>();
            db.WordEntries.RemoveRange(list.Where(l => l.IsFavorited == false && l.TagIdsJson.Count == 0 && l.Note == string.Empty ));
            await db.SaveChangesAsync(); 
        }
    }
}
