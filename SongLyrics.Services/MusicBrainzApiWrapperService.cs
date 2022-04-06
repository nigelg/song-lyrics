using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Microsoft.Extensions.Options;
using SongLyrics.Model;
using SongLyrics.Model.Settings;
using SongLyrics.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SongLyrics.Services
{
    public class MusicBrainzApiWrapperService : IMusicBrainzApiWrapperService
    {
        private readonly AppSettings _appSettings;
        private readonly Query _query;
        private readonly ILogger<MusicBrainzApiWrapperService> _logger;

        /// <summary>
        /// The max limit for results on API calls
        /// </summary>
        private const int _limit = 100;

        public MusicBrainzApiWrapperService(ILogger<MusicBrainzApiWrapperService> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _query = new Query(_appSettings.MusicBrainzUserAgentApplication, _appSettings.MusicBrainzUserAgentVersion, _appSettings.MusicBrainzUserAgentContact);
        }
        
        public MusicBrainzApiWrapperService() {}

        public async Task<Dictionary<int, Artist>?> GetArtistAsync(string search)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(search))
                {
                    return null;
                }

                var artists = await _query.FindArtistsAsync(search, simple: true, limit: _limit);

                var index = 1;
                var artistsDict = new Dictionary<int, Artist>();

                //iterate through all artists, paging the results using _limit
                for (var i = 0; i < Math.Ceiling((decimal)artists.TotalResults / _limit); i++)
                {
                    _logger.LogInformation($"Reading {artists.Offset + 1}-{artists.Offset + artists.Results.Count} of {artists.TotalResults} artists...");

                    foreach (var a in artists.Results)
                    {
                        artistsDict[index] = new Artist() { Id = a.Item.Id, Name = a.Item.Name, Detail = a.Item.Disambiguation };
                        index++;
                    }
                    artists.Next();
                }

                return artistsDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
        
        public async Task<Dictionary<string, Guid>?> GetArtistReleaseGroupsAsync(Guid mbid)
        {
            try
            {
                var artist = await _query.LookupArtistAsync(mbid, inc: Include.ReleaseGroups, type: ReleaseType.Album);

                var albumReleasesDict = new Dictionary<string, Guid>();

                //iterate through the releases (albums)
                foreach (var release in artist.ReleaseGroups)
                {
                    //make sure we don't add dupe albums
                    if (!albumReleasesDict.ContainsKey(release.Title) && release.SecondaryTypes.Count() == 0)
                    {
                        albumReleasesDict[release.Title] = release.Id;
                    }
                }
                _logger.LogInformation($"Found release-groups{Environment.NewLine}{Environment.NewLine}{String.Join(Environment.NewLine, albumReleasesDict.Keys.Select(x => x))}");
                return albumReleasesDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<List<Guid>?> GetArtistReleaseGroupFirstReleaseAsync(List<Guid> mbIds)
        {
            try
            {
                var firstReleases = new List<Guid>();
                var taskList = new List<Task<IReleaseGroup>>();

                foreach (var id in mbIds)
                {
                    taskList.Add(_query.LookupReleaseGroupAsync(id, inc: Include.Releases));
                }

                var allReleasesGroups = await Task.WhenAll(taskList.ToList()).ConfigureAwait(false);


                foreach (var releaseGroup in allReleasesGroups)
                {
                    firstReleases.Add(releaseGroup.Releases.FirstOrDefault().Id);
                }

                return firstReleases;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<List<string>?> GetAlbumsTracksAsync(List<Guid> mbIds)
        {
            try
            {
                var tracks = new List<string>();
                var taskList = new List<Task<IRelease>>();

                foreach (var id in mbIds)
                {
                    taskList.Add(_query.LookupReleaseAsync(id, inc: Include.Recordings));
                }

                var allReleases = await Task.WhenAll(taskList.ToList()).ConfigureAwait(false);

                var trackCountForLogging = 0;
                foreach (var release in allReleases)
                {
                    foreach (var media in release.Media)
                    {
                        foreach (var track in media.Tracks)
                        {
                            //make sure we don't add dupe tracks
                            //possible problem here if the same track name is used for different songs across different albums - but then the way LyricsOvh works would fail too.
                            if (!tracks.Contains(track.Title))
                            {
                                tracks.Add(track.Title);
                            }
                        }
                        var listOfAlbumTracks = String.Join($"{Environment.NewLine}", tracks.GetRange(trackCountForLogging, tracks.Count() - trackCountForLogging));
                        _logger.LogInformation($"Found {tracks.Count() - trackCountForLogging} track(s) from album '{release.Title.Trim()}'{Environment.NewLine}{Environment.NewLine}{listOfAlbumTracks}.");
                        trackCountForLogging = tracks.Count();
                    }
                }

                return tracks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}