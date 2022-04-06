namespace SongLyrics.Model.Settings
{
    public class AppSettings
    {
        public string MusicBrainzUserAgentApplication { get; set; }
        public string MusicBrainzUserAgentVersion { get; set; }
        public string MusicBrainzUserAgentContact { get; set; }
        public string LryicsApiBaseUrl { get; set; }
        public int LyricsOvhBatchProcessRequests { get; set; }
    }
}
