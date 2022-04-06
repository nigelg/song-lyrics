using Microsoft.Extensions.Logging;
using Moq;
using SongLyrics.Model;
using SongLyrics.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SongLyrics.Services.Tests
{
    public class SongLyricsServiceTests
    {
        ISongLyricsService _songLyricsService;
        Mock<ILogger<SongLyricsService>> _mockLogger = new Mock<ILogger<SongLyricsService>>();
        Mock<MusicBrainzApiWrapperService> _mockMusicBrainzApiWrapperService = new Mock<MusicBrainzApiWrapperService>();
        Mock<LyricsOvhApiService> _mockLyricsOvhApiService = new Mock<LyricsOvhApiService>();

        public SongLyricsServiceTests(){}

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData(" \t ")]
        [InlineData(" \r ")]
        [InlineData(" \n ")]
        [InlineData(" \n\r ")]
        public async Task GetArtistAsync_empty_search_returns_null(string search)
        {
            //Arrange

            //Act
            var result = await _songLyricsService.GetArtistAsync(search);

            //Assert
            Assert.Null(result);
        }
    }
}
