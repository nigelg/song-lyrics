using SongLyrics.Services.Interface;
using System;
using System.Collections.Generic;
using SongLyrics.Model.Helpers;
using System.Threading.Tasks;
using SongLyrics.Model;

public class App
{
    private readonly ISongLyricsService _songLyricsService;

    public App(ISongLyricsService songLyricsService)
    {
        _songLyricsService = songLyricsService;
    }

    public async Task Run()
    {
        var selectedArtist = await SearchAndSelectArtist();

        await GetAverageLyricsForArtist(selectedArtist);

        await Task.CompletedTask;
    }

    private async Task<Artist> SearchAndSelectArtist()
    {
        var artistsSearchResults = await SearchArtists();

        //ask user to search again if no results
        while (artistsSearchResults == null || artistsSearchResults.Count == 0)
        {
            Console.WriteLine($"Zero artists found, search again...");
            artistsSearchResults = await SearchArtists();
        }

        Console.WriteLine($"{Environment.NewLine}Artists found: {artistsSearchResults.Count}");

        return SelectArtist(artistsSearchResults);        
    }

    private Artist SelectArtist(Dictionary<int, Artist> artists)
    {
        var selectedIndex = "1";
        //if there are more than 1 artist then have the user select the artist, otherwise use the only available artist
        if (artists.Count > 1)
        {
            //iterate through all artists 
            foreach (var a in artists)
            {
                Console.WriteLine($"{Environment.NewLine}Index: {a.Key}{Environment.NewLine}Name: {a.Value.Name}");

                if (!String.IsNullOrEmpty(a.Value.Detail))
                {
                    Console.WriteLine($"Detail: {a.Value.Detail}");
                }
            }

            Console.WriteLine($"{Environment.NewLine}Use the artist index to confirm selection:");

            selectedIndex = Console.ReadLine();
        }

        while (!StringIntegersHelper.IsIntegerWithinRange(selectedIndex, max: artists.Count))
        {
            Console.WriteLine($"{Environment.NewLine}Selected index not valid, try again.");
            selectedIndex = Console.ReadLine();
        }

        return artists[Convert.ToInt32(selectedIndex)];
    }

    private async Task<Dictionary<int, Artist>> SearchArtists()
    {
        Console.WriteLine($"{Environment.NewLine}Search for an artist:");

        var artistSearchText = Console.ReadLine();

        Console.WriteLine($"{Environment.NewLine}Searching artists using \"{artistSearchText}\"... please wait.");

        return await _songLyricsService.GetArtistAsync($"\"{artistSearchText}\"");
    }

    private async Task GetAverageLyricsForArtist(Artist artist)
    {
        var dtStart = DateTime.Now;
        var averageWordCount = await _songLyricsService.GetAverageWordCountOfAlbumTracksAsync(artist.Id, artist.Name);
        TimeSpan span = DateTime.Now - dtStart;

        if (averageWordCount == null)
        {
            Console.WriteLine($"Failed to calculate average number of words in tracks. Exiting program");
            return;
        }

        Console.WriteLine($"The average word count per track for lyrics we were able to obtain across all albums is...");
        Console.WriteLine($"{Environment.NewLine}==={Environment.NewLine}{averageWordCount}{Environment.NewLine}===");

        Console.Write($"{Environment.NewLine}{Environment.NewLine}Got all lyrics in {span.TotalSeconds} seconds.{Environment.NewLine}{Environment.NewLine}");
    }
}