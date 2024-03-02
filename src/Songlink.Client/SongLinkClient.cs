using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using RestSharp;
using RestSharp.Serializers.Json;
using Songlink.Client.Abstractions;
using Songlink.Client.Configuration;
using Songlink.Client.Exceptions;
using Songlink.Client.Model;
using Songlink.Client.Model.Base;

namespace Songlink.Client;

public class SongLinkClient
{
    private readonly RestClient _restClient;

    private const string GET_SONG_LINKS_RESOURCE = "v1-alpha.1/links";

    private static readonly JsonSerializerOptions _serializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters = {new JsonStringEnumConverter(allowIntegerValues: true)}
        };

    public SongLinkClient(
        IHttpClientFactory httpClientFactory,
        ISonglinkConfigurationProvider? configurationProvider = null)
    {
        // TODO: add getting api key form configurationProvider

        var httpClient = httpClientFactory.CreateClient(SonglinkClientConfiguration.SonglinkHttpClientName);

        _restClient = new RestClient(
            httpClient,
            configureSerialization: s => s.UseSystemTextJson(_serializerOptions)
        );
    }

    public SongLinkClient(string apiUrl, string? apiKey = null)
    {
        var restClientOptions = new RestClientOptions()
        {
            BaseUrl = new Uri(apiUrl)
        };
        
        _restClient = new RestClient(
            restClientOptions,
            configureSerialization: s => s.UseSystemTextJson(_serializerOptions)
        );
    }

    public Task<SongLinkResponse> GetAllSongLinksAsync(
        Uri songShareUri,
        CancellationToken cancellationToken)
    {
        return GetAllSongLinksAsync(songShareUri.ToString(), cancellationToken);
    }

    public async Task<SongLinkResponse> GetAllSongLinksAsync(
        string songShareLink,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(songShareLink);

        var request = new RestRequest(GET_SONG_LINKS_RESOURCE);

        request.Parameters.AddParameter(
            Parameter.CreateParameter(
                "url",
                songShareLink,
                ParameterType.GetOrPost
            )
        );

        var response = await ExecuteRequestCore<SongLinkResponse>(request, cancellationToken);

        return response;
    }

    private async Task<TResponse> ExecuteRequestCore<TResponse>(
        RestRequest request,
        CancellationToken cancellationToken)
        where TResponse : SongLinkResponseBase
    {
        var response = await _restClient.ExecuteAsync<TResponse>(request, cancellationToken);

        if (response.IsSuccessful)
        {
            response.Data!.IsSuccess = true;

            return response.Data;
        }

        if (!response.IsSuccessStatusCode
            && response.StatusCode != HttpStatusCode.BadRequest
            && response.StatusCode != HttpStatusCode.NotFound
            && response.StatusCode != HttpStatusCode.Forbidden)
        {
            throw new SongLinkException(
                request.Method.ToString(),
                request.Resource,
                response.StatusCode,
                response.StatusDescription,
                response.Content);
        }

        var errorResponse = Activator.CreateInstance<TResponse>();

        errorResponse.IsSuccess = false;

        // here errorResponse.ErrorMessage always contains complaints about inability to create 
        // SonglinkResponse due to some required properties uninitialized
        errorResponse.ErrorMessage = response.Content;

        return errorResponse;
    }
}
