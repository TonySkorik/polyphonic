namespace Songlink.Client.Model.Base;

public class SongLinkResponseBase
{
    /// <summary>
    /// Indicates whether the request completed successfully.
    /// In case it does not - this property is se to <c>false</c> 
    /// and <see cref="ErrorMessage"/> contains the error explanation.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Contains the error message if the request did not complete successfully.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
