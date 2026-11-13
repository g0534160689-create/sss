namespace DaatAharon.Core.Links;

public static class LinkAddress
{
    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || value.Length > 2048 ||
            value.Any(char.IsControl) || !Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
            return false;

        if ((uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) ||
            string.IsNullOrWhiteSpace(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo))
            return false;

        normalized = uri.AbsoluteUri;
        return true;
    }
}
