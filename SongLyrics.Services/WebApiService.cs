using Newtonsoft.Json;
using SongLyrics.Model.Helpers;
using SongLyrics.Services.Interface;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace SongLyrics.Services
{
    public class WebApiService : IWebApiService
    {
        protected HttpClientHandler handler { get; set; }
        protected CookieContainer cookieJar { get; set; }
        protected HttpClient? client { get; set; }

        public WebApiService()
        {
            cookieJar = new CookieContainer();
            handler = new HttpClientHandler { CookieContainer = cookieJar };
        }

        public void SetBaseAddress(Uri baseAddress)
        {
            client = new HttpClient(handler) { BaseAddress = baseAddress };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected string EncodeParameter(string key, object value)
        {
            if (value == null) return String.Concat(key, "=&");

            if (value is String[])
            {
                value = String.Join(",", (String[])value);
            }

            return String.Concat(key, "=", HttpUtility.UrlEncode(value.ToString()), "&");
        }

        public async Task<ApiResult<T>> GetAsync<T>(string uri, object? data = null)
        {
            var qs = "";
            if (data != null)
            {
                qs = String.Format("?{0}", GetQueryString(data));
            }

            HttpResponseMessage response = null;

            var result = new ApiResult<T>();
            result.Url = uri + qs;

            response = await client.GetAsync(uri + qs);            
            result.HttpStatusCode = response.StatusCode;

            try
            {
                result.Data = await response.Content.ReadAsJsonAsync<T>();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                //TODO:Log message
            }
            return result;
        }

        

        private string GetQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                                where p.GetValue(obj, null) != null
                                select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }
    }

    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            string json = await content.ReadAsStringAsync();
            T value = JsonConvert.DeserializeObject<T>(json);
            return value;
        }
    }
}