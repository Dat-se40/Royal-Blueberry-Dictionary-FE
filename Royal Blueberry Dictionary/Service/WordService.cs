using BlueBerryDictionary.Helpers;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
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
            var newEntry  = await WordEntryRepository.GetByWordAndMeaningAsync("GUEST",detail.Word, meaningIdx); 
            
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
        public async Task<WordEntry> GetWordEntryByID (string ID) 
        {
            var res = await WordEntryRepository.GetByIdAsync(ID);
            return res;
        }
        public async void DeletedEntry(string ID)
        {
            await WordEntryRepository.DeleteAsync(ID); 
        }
        public static WordEntry MapWordDetailToWordEntry(WordDetail detail, int meaningIdx, int defIdx)
        {
            var meaning = detail.Meanings[meaningIdx];
            var definition = meaning.Definitions[defIdx];

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
        public async Task FavoriteAsync(WordEntry wordEntry)
        {
            wordEntry.IsFavorited = !wordEntry.IsFavorited;
            await SmartUpdate(wordEntry); 
            
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
    }
}
