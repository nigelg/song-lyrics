using SongLyrics.Model.Helpers;
using System;
using System.Threading.Tasks;

namespace SongLyrics.Services.Interface
{
    public interface IWebApiService
    {
        void SetBaseAddress(Uri baseAddress);
        Task<ApiResult<T>> GetAsync<T>(string uri, object? data = null);
    }
}
