using FluentAssertions;

using Songlink.Client;

namespace Polyphonic.Tests.TestClasses;

internal class SongLinkClientTests
{
    private SongLinkClient _songLinkClient;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _songLinkClient = new SongLinkClient("https://api.song.link/");
    }

    [Test]
    public async Task TestGetAllSongLinks_SongNotFound()
    {
        var response = await _songLinkClient.GetAllSongLinksAsync(
            "https://open.spotify.com/track/AAAA", // not found by original share link
            CancellationToken.None);

        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("404");
    }

    [Test]
    public async Task TestGetAllSongLinks_InvalidUrl()
    {
        var response = await _songLinkClient.GetAllSongLinksAsync(
            "AAAA", // invalid url
            CancellationToken.None);

        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("400");
    }

    [Test]
    public async Task TestGetAllSongLinks_Success()
    {
        var response = await _songLinkClient.GetAllSongLinksAsync(
            "https://open.spotify.com/track/0PUa24Pxvvxa3ys7reyGSx",
            CancellationToken.None);

        response.Should().NotBeNull();

        response.IsSuccess.Should().BeTrue();
        response.ErrorMessage.Should().BeNull();

        response.EntitiesByUniqueId.Count.Should().BeGreaterThan(0);
        response.LinksByPlatform.Count.Should().BeGreaterThan(0);

        response.PageUrl.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task TestGetAllSongLinks_AlbumLink_Success()
    {
        var response = await _songLinkClient.GetAllSongLinksAsync(
            "https://open.spotify.com/album/5SMhoGL3lWmlYagofVSBwL?si=s_aJlaIlQe2RWWSIKpd1Zg",
            CancellationToken.None);

        response.Should().NotBeNull();

        response.IsSuccess.Should().BeTrue();
        response.ErrorMessage.Should().BeNull();

        response.EntitiesByUniqueId.Count.Should().BeGreaterThan(0);
        response.LinksByPlatform.Count.Should().BeGreaterThan(0);

        response.PageUrl.Should().NotBeNullOrEmpty();
    }
}
