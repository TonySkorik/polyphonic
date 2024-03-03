using Songlink.Client.Model.Base;

namespace Songlink.Client.Model;

/* FROM https://linktree.notion.site/API-d0ebe08a5e304a55928405eb682f6741
// @flow

type Response = {
  // The unique ID for the input entity that was supplied in the request. The
  // data for this entity, such as title, artistName, etc. will be found in
  // an object at `nodesByUniqueId[entityUniqueId]`
  entityUniqueId: string,

  // The userCountry query param that was supplied in the request. It signals
  // the country/availability we use to query the streaming platforms. Defaults
  // to 'US' if no userCountry supplied in the request.
  //
  // NOTE: As a fallback, our service may respond with matches that were found
  // in a locale other than the userCountry supplied
  userCountry: string,

  // A URL that will render the Songlink page for this entity
  pageUrl: string,

  // A collection of objects. Each key is a platform, and each value is an
  // object that contains data for linking to the match
  linksByPlatform: {
    // Each key in `linksByPlatform` is a Platform. A Platform will exist here
    // only if there is a match found. E.g. if there is no YouTube match found,
    // then neither `youtube` or `youtubeMusic` properties will exist here
    [Platform]: {
      // The unique ID for this entity. Use it to look up data about this entity
      // at `entitiesByUniqueId[entityUniqueId]`
      entityUniqueId: string,

      // The URL for this match
      url: string,

      // The native app URI that can be used on mobile devices to open this
      // entity directly in the native app
      nativeAppUriMobile?: string,

      // The native app URI that can be used on desktop devices to open this
      // entity directly in the native app
      nativeAppUriDesktop?: string,
    },
  },

  // A collection of objects. Each key is a unique identifier for a streaming
  // entity, and each value is an object that contains data for that entity,
  // such as `title`, `artistName`, `thumbnailUrl`, etc.
  entitiesByUniqueId: {
    [entityUniqueId]: {
      // This is the unique identifier on the streaming platform/API provider
      id: string,

      type: 'song' | 'album',

      title?: string,
      artistName?: string,
      thumbnailUrl?: string,
      thumbnailWidth?: number,
      thumbnailHeight?: number,

      // The API provider that powered this match. Useful if you'd like to use
      // this entity's data to query the API directly
      apiProvider: APIProvider,

      // An array of platforms that are "powered" by this entity. E.g. an entity
      // from Apple Music will generally have a `platforms` array of
      // `["appleMusic", "itunes"]` since both those platforms/links are derived
      // from this single entity
      platforms: Platform[],
    },
  },
};

type Platform =
  | 'spotify'
  | 'itunes'
  | 'appleMusic'
  | 'youtube'
  | 'youtubeMusic'
  | 'google'
  | 'googleStore'
  | 'pandora'
  | 'deezer'
  | 'tidal'
  | 'amazonStore'
  | 'amazonMusic'
  | 'soundcloud'
  | 'napster'
  | 'yandex'
  | 'spinrilla'
  | 'audius'
  | 'audiomack'
  | 'anghami'
  | 'boomplay';

type APIProvider =
  | 'spotify'
  | 'itunes'
  | 'youtube'
  | 'google'
  | 'pandora'
  | 'deezer'
  | 'tidal'
  | 'amazon'
  | 'soundcloud'
  | 'napster'
  | 'yandex'
  | 'spinrilla'
  | 'audius'
  | 'audiomack'
  | 'anghami'
  | 'boomplay';
*/

public class SongLinkResponse : SongLinkResponseBase
{
    /// <summary>
    /// The unique ID for the input entity that was supplied in the request. 
    /// The data for this entity, such as title, artistName, etc. will be found in 
    /// an object at `nodesByUniqueId[entityUniqueId]`.
    /// </summary>
    public required string EntityUniqueId { set; get; }

    /// <summary>
    /// The userCountry query param that was supplied in the request. It signals
    /// the country/availability we use to query the streaming platforms. Defaults
    /// to 'US' if no userCountry supplied in the request.
    /// NOTE: As a fallback, our service may respond with matches that were found
    /// in a locale other than the userCountry supplied.
    /// </summary>
    public required string UserCountry { set; get; }

    /// <summary>
    /// A URL that will render the Songlink page for this entity.
    /// </summary>
    public required string PageUrl { set; get; }

    /// <summary>
    /// Each key in `linksByPlatform` is a Platform. A Platform will exist here 
    /// only if there is a match found. E.g. if there is no YouTube match found,
    /// then neither `youtube` or `youtubeMusic` properties will exist here.
    /// </summary>
    public required Dictionary<SongLinkPlatform, PlatformSongLinks> LinksByPlatform { set; get; }

    /// <summary>
    /// A collection of objects. Each key is a unique identifier for a streaming
    /// entity, and each value is an object that contains data for that entity,
    /// such as `title`, `artistName`, `thumbnailUrl`, etc.
    /// </summary>
    public required Dictionary<string, SongLinkEntity> EntitiesByUniqueId { set; get; }

    public class PlatformSongLinks
    {
        /// <summary>
        /// The unique ID for this entity. Use it to look up data about this entity
        /// at `entitiesByUniqueId[entityUniqueId]`.
        /// </summary>
        public required string EntityUniqueId { set; get; }

        /// <summary>
        /// The URL for this match.
        /// </summary>
        public required string Url { set; get; }

        /// <summary>
        /// The native app URI that can be used on mobile devices to open this
        /// entity directly in the native app.
        /// </summary>
        public string? NativeAppUriMobile { set; get; }

        /// <summary>
        /// The native app URI that can be used on desktop devices to open this
        /// entity directly in the native app. 
        /// </summary>
        public string? NativeAppUriDesktop { set; get; }
    }

    public class SongLinkEntity
    {
        /// <summary>
        /// This is the unique identifier on the streaming platform/API provider.
        /// </summary>
        public required string Id { set; get; }

        /// <summary>
        /// The entity type.
        /// </summary>
        public SongLinkEntityType Type { set; get; }

        public string? Title { set; get; }

        public string? ArtistName { set; get; }

        public string? ThumbnailUrl { set; get; }

        public int? ThumbnailWidth { set; get; }

        public int? ThumbnailHeight { set; get; }

        /// <summary>
        /// The API provider that powered this match. Useful if you'd like to use
        /// this entity's data to query the API directly.
        /// </summary>
        public SongLinkApiProvider ApiProvider { set; get; }

        /// <summary>
        /// An array of platforms that are "powered" by this entity. E.g. an entity
        /// from Apple Music will generally have a `platforms` array of
        /// `["appleMusic", "itunes"]` since both those platforms/links are derived
        /// from this single entity.
        /// </summary>
        public required SongLinkPlatform[] Platforms { set; get; }
    }
}
