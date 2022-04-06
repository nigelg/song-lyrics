using SongLyrics.Model;
using SongLyrics.Model.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SongLyrics.Services.Interface
{
    public interface ILyricsOvhApiService
    {

        /// <summary>
        /// Get The lyrics for a single song.
        /// </summary>
        /// <param name="artist">The artists name</param>
        /// <param name="songTitle">The song title belonging to the artist</param>
        /// <returns>An ApiResult<LyricsOvhRoot> object, where ApiResult.Data is LyricsOvhRoot</returns>
        Task<ApiResult<LyricsOvhRoot>> GetLyricsAsync(string artist, string songTitle);

        /// <summary>
        /// Get The lyrics for a list of songs belonging to the same artist.
        /// </summary>
        /// <param name="artist">The artists name</param>
        /// <param name="songTitle">The list of song titles belonging to the artist</param>
        /// <returns>A List<ApiResult<LyricsOvhRoot>> object, where each ApiResult.Data is LyricsOvhRoot</returns>
        Task<List<ApiResult<LyricsOvhRoot>>> GetLyricsAsync(string artist, List<string> songTitle);
    }
}