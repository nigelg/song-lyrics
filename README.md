# song-lyrics

This application allows you find the mean average of lyrics (words) contained across all albums of a single artist.

## assumptions

To obtain tracks we get a list of albums by an artists. These albums only include official releases and not other releases such as singles, audio drama, audiobook, broadcast, compilation, dj-mix, interview, live compliations etc.

## usage

 - Clone the repository.
 - Open the solution using a Visual Studio version that supports development with the .Net Core 3.1 SDK. Visual Studio 2022 was used for development. You may need to install the .Net Core 3.1 SDK.
 - Using the file in \SongLyrics.Cli\appsettings.json please modify the MusicBrainzUserAgentContact string so you can be identifiable to the MusicBrainz API when running the application.
 - Run the SongLyrics.Cli project application and follow the on-screen prompts.