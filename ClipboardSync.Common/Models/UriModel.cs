namespace ClipboardSync.Common.Models
{
    /// <summary>
    /// Be used to pass root uri from razor page to backend file service to access file web api.
    /// </summary>
    public class UriModel
    {
        public string? RootUri { get; set; }
    }
}
