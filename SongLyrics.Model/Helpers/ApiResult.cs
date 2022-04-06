using System;
using System.Net;
using Newtonsoft.Json;

namespace SongLyrics.Model.Helpers
{
    [JsonObject]
    [Serializable]
    public class ApiResult<T>
    {
        public T Data { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string Url { get; set; }
    }
}