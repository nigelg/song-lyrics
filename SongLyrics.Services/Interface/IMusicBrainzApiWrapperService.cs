
using SongLyrics.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SongLyrics.Services.Interface
{
    public interface IMusicBrainzApiWrapperService
    {
        /// <summary>
        /// Gets all artists applicable to the specified search
        /// </summary>
        /// <param name="search">Search phrase to search using, e.g. Oasis</param>
        /// <returns>Dictionary<int, Artist></returns>
        Task<Dictionary<int, Artist>?> GetArtistAsync(string search);

        /// <summary>
        /// Get all release-group MusicBrainz Ids associated with an artist
        /// </summary>
        /// <param name="mid">The artist MusicBrainz Id</param>
        /// <returns>Dictionary<string, Guid></returns>
        Task<Dictionary<string, Guid>?> GetArtistReleaseGroupsAsync(Guid id);

        /// <summary>
        /// Get each first release MusicBrainz Id associated with a list of release-groups
        /// </summary>
        /// <param name="mbIds">The list of release-group MusicBrainz Ids</param>
        /// <returns></returns>
        Task<List<Guid>?> GetArtistReleaseGroupFirstReleaseAsync(List<Guid> mbIds);

        /// <summary>
        /// Get a list of track titles associated with a list of releases
        /// </summary>
        /// <param name="mbIds">The list of release MusicBrainz Ids</param>
        /// <returns></returns>
        Task<List<string>?> GetAlbumsTracksAsync(List<Guid> id);
    }
}
