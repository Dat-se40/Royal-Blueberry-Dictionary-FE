using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Word;
using Royal_Blueberry_Dictionary.Repository.Interface;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service
{
    public class TagService
    {
        private readonly ITagRepository _tagRepo;
        private readonly IWordEntryRepository _wordRepo;
        private readonly IBackendApiClient _apiClient;
        private readonly SearchService _searchService; 

        public TagService(
            ITagRepository tagRepo,
            IWordEntryRepository wordRepo,
            IBackendApiClient apiClient,
            SearchService searchService)
        {
            _tagRepo = tagRepo;
            _wordRepo = wordRepo;
            _apiClient = apiClient;
            _searchService = searchService;
        }

        private string GetEffectiveId(string userId) => string.IsNullOrEmpty(userId) ? "GUEST" : userId;

        #region Synchronization (Server <-> Local)

        /// <summary>
        /// Đồng bộ: Cả Tags và WordTagRelations (Class Meta)
        /// </summary>
        public async Task SyncAllWithServerAsync(string userId)
        {
            string uid = GetEffectiveId(userId);
            if (uid == "GUEST") return; // Không sync nếu là khách

            await SyncTagsToServerAsync(uid);
            await SyncRelationsToServerAsync(uid);
        }

        private async Task SyncTagsToServerAsync(string userId)
        {
            var dirtyTags = await _tagRepo.GetDirtyTagsAsync(userId);
            foreach (var tag in dirtyTags)
            {
                var response = await _apiClient.PostAsync<Tag>("tags/sync", tag);
                if (response != null)
                {
                    tag.IsDirty = false;
                    await _tagRepo.UpdateTagAsync(tag);
                }
            }
            await _tagRepo.SaveChangesAsync();
        }

        private async Task SyncRelationsToServerAsync(string userId)
        {
            var dirtyRels = await _tagRepo.GetDirtyRelationsAsync(userId);
            foreach (var rel in dirtyRels)
            {
                // Chỉ gửi metadata nhỏ (UserId, Word, MeaningIndex, TagId)
                var success = await _apiClient.PostAsync<bool>("relations/sync", rel);
                if (success)
                {
                    rel.IsDirty = false;
                }
            }
            await _tagRepo.SaveChangesAsync();
        }

        /// <summary>
        /// Tải dữ liệu từ Server về (Sử dụng khi đăng nhập máy mới)
        /// </summary>
        public async Task FetchEverythingFromServerAsync(string userId)
        {
            // 1. Tải Tags
            var remoteTags = await _apiClient.GetAsync<List<Tag>>($"tags/user/{userId}");
            if (remoteTags != null)
            {
                foreach (var rTag in remoteTags)
                {
                    if (await _tagRepo.GetTagByIdAsync(rTag.Id) == null)
                    {
                        rTag.IsDirty = false;
                        await _tagRepo.AddTagAsync(rTag);
                    }
                }
            }

            // 2. Tải Relations (Meta)
            var remoteRels = await _apiClient.GetAsync<List<WordTagRelation>>($"relations/user/{userId}");
            if (remoteRels != null)
            {
                foreach (var rRel in remoteRels)
                {
                    rRel.IsDirty = false;
                    await _tagRepo.AddRelationAsync(rRel);
                }
            }

            await _tagRepo.SaveChangesAsync();
        }

        #endregion

        #region Business Logic (UI Helpers)

        /// <summary>
        /// Lấy danh sách WordEntry thuộc một Tag. 
        /// Nếu Local chưa có (máy mới), sẽ tự động fetch từ API qua SearchService.
        /// </summary>
        public async Task<List<WordEntry>> GetWordsInTagAsync(string userId, string tagId)
        {
            var relations = await _tagRepo.GetRelationsByTagAsync(tagId);
            var result = new List<WordEntry>();

            foreach (var rel in relations)
            {
                var localEntry = await _wordRepo.GetByWordAndMeaningAsync(userId, rel.Word, rel.MeaningIndex);

                if (localEntry != null)
                {
                    result.Add(localEntry);
                }
                else
                {
                    var detail = await _searchService.searchAWord(rel.Word);
                    if (detail != null)
                    {
                        var newEntry = new WordEntry().MapWordDetailToWordEntry(detail, rel.MeaningIndex, 0);
                        newEntry.UserId = GetEffectiveId(userId);
                        await _wordRepo.AddAsync(newEntry);
                        result.Add(newEntry);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gắn tag cho một từ. Tạo một bản ghi Relation (Meta).
        /// </summary>
        public async Task LinkWordToTagAsync(string userId, string word, int meaningIndex, string tagId)
        {
            var relation = new WordTagRelation
            {
                UserId = GetEffectiveId(userId),
                Word = word,
                MeaningIndex = meaningIndex,
                TagId = tagId,
                IsDirty = true
            };
            await _tagRepo.AddRelationAsync(relation);
            await _tagRepo.SaveChangesAsync();
        }

        #endregion
    }
}