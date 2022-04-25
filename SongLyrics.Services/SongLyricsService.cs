using Microsoft.Extensions.Logging;
using SongLyrics.Model;
using SongLyrics.Model.Helpers;
using SongLyrics.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SongLyrics.Services
{
    public class SongLyricsService : ISongLyricsService
    {
        private readonly IMusicBrainzApiWrapperService _musicBrainzApiWrapperService;
        private readonly ILyricsOvhApiService _lyricsOvhApiService;
        private readonly ILogger<SongLyricsService> _logger;

        public SongLyricsService(ILogger<SongLyricsService> logger, IMusicBrainzApiWrapperService musicBrainzApiWrapperService, ILyricsOvhApiService lyricsOvhApiService)
        {
            _logger = logger;
            _musicBrainzApiWrapperService = musicBrainzApiWrapperService;
            _lyricsOvhApiService = lyricsOvhApiService;
        }

        public async Task<Dictionary<int, Artist>?> GetArtistAsync(string search)
        {
            return await _musicBrainzApiWrapperService.GetArtistAsync(search);
        }

        public async Task<double?> GetAverageWordCountOfAlbumTracksAsync(Guid mbId, string artistName)
        {
            #region GetArtistReleaseGroups

            _logger.LogInformation($"{Environment.NewLine}Getting release-groups (albums) for artist... please wait.");
            var releaseGroups = await _musicBrainzApiWrapperService.GetArtistReleaseGroupsAsync(mbId);

            if (releaseGroups == null || releaseGroups.Count == 0)
            {
                _logger.LogInformation($"Zero release-groups (albums) found.");
                return null;
            }

            #endregion

            #region GetReleaseGroupsFirstRelease

            _logger.LogInformation($"{Environment.NewLine}Getting first release of each release-group (album) contained within {releaseGroups.Count} albums... please wait.");

            var firstReleaseOfEachReleaseGroup = await _musicBrainzApiWrapperService.GetArtistReleaseGroupFirstReleaseAsync(new List<Guid>(releaseGroups.Values));

            if (firstReleaseOfEachReleaseGroup == null || firstReleaseOfEachReleaseGroup.Count == 0)
            {
                _logger.LogInformation($"Zero first releases for any release-groups (albums) found.");
                return null;
            }

            #endregion

            #region GetReleaseTracks

            _logger.LogInformation($"{Environment.NewLine}Getting tracks contained within {releaseGroups.Count} release(s) (albums)... please wait.");

            var artistAlbumsTracks = await _musicBrainzApiWrapperService.GetAlbumsTracksAsync(new List<Guid>(firstReleaseOfEachReleaseGroup));

            if (artistAlbumsTracks == null || artistAlbumsTracks.Count == 0)
            {
                _logger.LogInformation($"Zero tracks found");
                return null;
            }

            #endregion

            #region GetLyrics

            _logger.LogInformation($"{Environment.NewLine}Getting lyrics contained within {artistAlbumsTracks.Count} tracks across {firstReleaseOfEachReleaseGroup.Count} albums... please wait a little longer.");
            var allTrackLyrics = await _lyricsOvhApiService.GetLyricsAsync(artistName, artistAlbumsTracks);

            return GectLyricsAverage(allTrackLyrics);

            #endregion
        }

        internal double GectLyricsAverage(List<ApiResult<LyricsOvhRoot>> lyrics)
        {
            var goodApiCalls = lyrics.Where(x => x.HttpStatusCode == HttpStatusCode.OK);

            //should catch multiple whitespace (e.g. tabs, newlines, etc.
            var listOfLyricsWithReplacedSpaces = goodApiCalls.Select(x => Regex.Replace(x.Data.Lyrics, @"\s+", " "));
            var listOfWordCounts = listOfLyricsWithReplacedSpaces.Select(x => x.Split(" ").Count());

            return Math.Round(listOfWordCounts.Average());
        }
    }
}