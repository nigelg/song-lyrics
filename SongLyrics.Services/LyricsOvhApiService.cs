using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SongLyrics.Model;
using SongLyrics.Model.Helpers;
using SongLyrics.Model.Settings;
using SongLyrics.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SongLyrics.Services
{
    public class LyricsOvhApiService : ILyricsOvhApiService
    {
        private readonly AppSettings _appSettings;
        private readonly IWebApiService _webApiService;
        private readonly ILogger<LyricsOvhApiService> _logger;

        public LyricsOvhApiService(){}

        public LyricsOvhApiService(ILogger<LyricsOvhApiService> logger, IOptions<AppSettings> appSettings, IWebApiService webApiService)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _webApiService = webApiService;
            _webApiService.SetBaseAddress(new Uri(appSettings.Value.LryicsApiBaseUrl));
        }

        public async Task<ApiResult<LyricsOvhRoot>> GetLyricsAsync(string artist, string songTitle)
        {
            var result = await GetLyricsAsync(artist, new List<string>() { songTitle });

            return result.FirstOrDefault();
        }

        public async Task<List<ApiResult<LyricsOvhRoot>>> GetLyricsAsync(string artist, List<string> songTitles)
        {
            var batchProcessWebCalls = _appSettings.LyricsOvhBatchProcessRequests;
            var taskList = new List<Task<ApiResult<LyricsOvhRoot>>>();
            var allLyrics = new List<ApiResult<LyricsOvhRoot>>();

            for (var i = 0; i < songTitles.Count; i++)
            {
                if (i == 0)
                {
                    var maxRangeNotExceedingSongTitleCount = i + batchProcessWebCalls < songTitles.Count ? i + batchProcessWebCalls : songTitles.Count;
                    _logger.LogInformation($"Searching lyrics for {i + 1}-{maxRangeNotExceedingSongTitleCount} of {songTitles.Count} tracks.");
                }

                taskList.Add(_webApiService.GetAsync<LyricsOvhRoot>($"{_appSettings.LryicsApiBaseUrl}/{HttpUtility.UrlEncode(artist)}/{HttpUtility.UrlEncode(songTitles[i])}"));

                if ((i+1) % batchProcessWebCalls == 0 && i != 0)
                {
                    var maxRangeNotExceedingSongTitleCount = i + batchProcessWebCalls < songTitles.Count ? i + batchProcessWebCalls + 1 : songTitles.Count;
                    _logger.LogInformation($"Searching lyrics for {i + 2}-{maxRangeNotExceedingSongTitleCount} of {songTitles.Count} tracks.");
                    var batchResult = await Task.WhenAll(taskList.ToList()).ConfigureAwait(false);
                    allLyrics.AddRange(batchResult);
                    taskList.Clear();
                }
            }

            //process any remaining requests
            if (taskList.Count > 0)
            {
                var batchResult = await Task.WhenAll(taskList.ToList()).ConfigureAwait(false);
                allLyrics.AddRange(batchResult);
            }
            
            GetLyricsAsyncLogInformation(allLyrics);

            return allLyrics.ToList();
        }

        private void GetLyricsAsyncLogInformation(List<ApiResult<LyricsOvhRoot>> apiCallResults)
        {
            var statusCountOk = apiCallResults.Where(x => x.HttpStatusCode == HttpStatusCode.OK).Count();
            var statusCountNotFound = apiCallResults.Where(x => x.HttpStatusCode == HttpStatusCode.NotFound).Count();
            var statusCountOthers = apiCallResults.Where(x => x.HttpStatusCode != HttpStatusCode.OK && x.HttpStatusCode != HttpStatusCode.NotFound).Count();

            var sb = new StringBuilder("");

            if (statusCountOk > 0)
            {
                sb.Append($"{statusCountOk} / {apiCallResults.Count()} track lyrics were found.{Environment.NewLine}");
            }
            if (statusCountNotFound > 0)
            {
                sb.Append($"{statusCountNotFound} / {apiCallResults.Count()} track lyrics were not found.{Environment.NewLine}");
            }
            if (statusCountOthers > 0)
            {
                sb.Append($"{statusCountOthers} / {apiCallResults.Count()} track lyrics requests failed to process.{Environment.NewLine}");
            }

            if (statusCountOk + statusCountNotFound + statusCountOthers > 0)
            {
                _logger.LogInformation(sb.ToString());
            }
        }
    }
}