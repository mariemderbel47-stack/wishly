using HtmlAgilityPack;
using System.Xml;

namespace Wishly.Infrastructure.Services;

public class LinkPreviewResult
{
    public string? Titre { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? Prix { get; set; }
}

public class LinkPreviewService
{
    private readonly HttpClient _httpClient;

    public LinkPreviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<LinkPreviewResult> GetPreviewAsync(string url)
    {
        var result = new LinkPreviewResult();

        try
        {
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            result.Titre = GetMetaContent(doc, "og:title")
                ?? doc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim();

            var imageUrl = GetMetaContent(doc, "og:image");
            result.PhotoUrl = ResoudreUrlAbsolue(imageUrl, url);

            var prixTexte = GetMetaContent(doc, "product:price:amount")
                ?? GetMetaContent(doc, "og:price:amount");

            if (decimal.TryParse(prixTexte, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var prix))
            {
                result.Prix = prix;
            }
        }
        catch
        {
            // Si le site bloque ou est inaccessible, on renvoie juste un résultat vide.
        }

        return result;
    }

    private static string? GetMetaContent(HtmlDocument doc, string property)
    {
        var node = doc.DocumentNode.SelectSingleNode($"//meta[@property='{property}']")
            ?? doc.DocumentNode.SelectSingleNode($"//meta[@name='{property}']");
        return node?.GetAttributeValue("content", null);
    }

    private static string? ResoudreUrlAbsolue(string? urlImage, string urlPage)
    {
        if (string.IsNullOrWhiteSpace(urlImage))
        {
            return null;
        }

        if (Uri.TryCreate(urlImage, UriKind.Absolute, out var absolue))
        {
            return absolue.ToString();
        }

        if (Uri.TryCreate(new Uri(urlPage), urlImage, out var resolue))
        {
            return resolue.ToString();
        }

        return null;
    }
}