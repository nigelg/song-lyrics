using SongLyrics.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SongLyrics.Services.Interface
{
    public interface ISongLyricsService
    {
        /// <summary>
        /// Gets all artists applicable to the specified search
        /// </summary>
        /// <param name="search">Search phrase to search using, e.g. Otis Redding</param>
        /// <returns>Dictionary<int, Artist></returns>
        Task<Dictionary<int, Artist>?> GetArtistAsync(string search);

        /// <summary>
        /// Get the mean average word count for an artist across each of their albums
        /// </summary>
        /// <param name="mid">The artist MusicBrainz Id</param>
        /// <param name="artistName">The artists name</param>
        /// <returns></returns>
        Task<double?> GetAverageWordCountOfAlbumTracksAsync(Guid mbId, string artistName);
    }
}
