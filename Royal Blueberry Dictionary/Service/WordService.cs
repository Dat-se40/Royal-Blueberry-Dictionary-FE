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
        public async Task<WordEntry?> GetWordEntryByDetail(WordDetail detail, int meaningIdx, int defIdx)
        {
            try
            {
                var existing = await WordEntryRepository.GetByWordAndMeaningAsync(App.UserId, detail.Word, meaningIdx);
                if (existing != null)
                {
                    return existing;
                }

                // Chỉ map trong memory để hiển thị — không ghi DB cho đến khi user Save/Favorite/Note.
                return MapWordDetailToWordEntry(detail, meaningIdx, defIdx);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public Task<WordEntry?> GetExistingEntryAsync(string word, int meaningIndex) =>
            WordEntryRepository.GetByWordAndMeaningAsync(App.UserId, word, meaningIndex);

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
            if (wordEntry == null || string.IsNullOrWhiteSpace(wordEntry.Word))
            {
                return;
            }

            var existing = await WordEntryRepository.GetByIdAsync(wordEntry.Id);
            if (existing == null)
            {
                existing = await WordEntryRepository.GetByWordAndMeaningAsync(
                    App.UserId,
                    wordEntry.Word,
                    wordEntry.MeaningIndex);
            }

            if (existing == null)
            {
                await WordEntryRepository.AddAsync(wordEntry);
                return;
            }

            existing.Note = wordEntry.Note;
            existing.IsFavorited = wordEntry.IsFavorited;

            if (!string.IsNullOrWhiteSpace(wordEntry.Definition))
            {
                existing.Definition = wordEntry.Definition;
            }

            if (!string.IsNullOrWhiteSpace(wordEntry.Example))
            {
                existing.Example = wordEntry.Example;
            }

            if (!string.IsNullOrWhiteSpace(wordEntry.Phonetic))
            {
                existing.Phonetic = wordEntry.Phonetic;
            }

            if (!string.IsNullOrWhiteSpace(wordEntry.PartOfSpeech))
            {
                existing.PartOfSpeech = wordEntry.PartOfSpeech;
            }

            MergeTagIds(existing, wordEntry);
            await WordEntryRepository.UpdateAsync(existing);
            wordEntry.Id = existing.Id;
        }

        private static void MergeTagIds(WordEntry existing, WordEntry incoming)
        {
            if (incoming.TagIdsJson == null || incoming.TagIdsJson.Count == 0)
            {
                return;
            }

            existing.TagIdsJson ??= new List<string>();
            foreach (var tagId in incoming.TagIdsJson)
            {
                if (!existing.TagIdsJson.Contains(tagId))
                {
                    existing.TagIdsJson.Add(tagId);
                }
            }
        }
        public async Task CleanUpData()
        {
            var list = await WordEntryRepository.GetAllAsync(App.UserId);
            var db = App.serviceProvider.GetRequiredService<AppDbContext>();

            var duplicateGroups = list
                .GroupBy(e => $"{e.Word.ToLower()}|{e.MeaningIndex}", StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateGroups)
            {
                var keeper = group
                    .OrderByDescending(e => !string.IsNullOrWhiteSpace(e.Note))
                    .ThenByDescending(e => e.IsFavorited)
                    .ThenByDescending(e => e.TagIdsJson?.Count ?? 0)
                    .ThenByDescending(e => e.LastModifiedAt)
                    .First();

                foreach (var duplicate in group.Where(e => e.Id != keeper.Id))
                {
                    keeper.Note = string.IsNullOrWhiteSpace(keeper.Note) ? duplicate.Note : keeper.Note;
                    keeper.IsFavorited |= duplicate.IsFavorited;
                    MergeTagIds(keeper, duplicate);
                    db.WordEntries.Remove(duplicate);
                }
            }

            db.WordEntries.RemoveRange(list.Where(l =>
                l.IsFavorited == false &&
                (l.TagIdsJson == null || l.TagIdsJson.Count == 0) &&
                string.IsNullOrWhiteSpace(l.Note)));
            await db.SaveChangesAsync();
        }
    }
}
