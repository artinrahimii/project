using System.IO;
using System.Text;

namespace WhiteP.Host.Windows.Models.Catalog;

/// <summary>Loads a local catalog with a byte ceiling and strict UTF-8 before metadata validation.</summary>
public sealed class ModelCatalogFileLoader
{
    public ModelCatalog Load(string path, IEnumerable<string> allowedSourceHosts,
        IEnumerable<string> supportedBackendIds, IEnumerable<string>? allowedLicenseHosts = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(allowedSourceHosts);
        ArgumentNullException.ThrowIfNull(supportedBackendIds);
        using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (input.Length > ModelCatalogParser.MaximumCatalogBytes)
            throw new InvalidDataException("Model catalog exceeds the configured byte limit.");
        using var output = new MemoryStream((int)input.Length);
        var buffer = new byte[8192];
        while (true)
        {
            var read = input.Read(buffer, 0, buffer.Length);
            if (read == 0) break;
            if (output.Length + read > ModelCatalogParser.MaximumCatalogBytes)
                throw new InvalidDataException("Model catalog exceeds the configured byte limit.");
            output.Write(buffer, 0, read);
        }
        string json;
        try
        {
            json = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(output.ToArray());
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException("Model catalog must be valid UTF-8.", exception);
        }
        if (json.Length > 0 && json[0] == '\uFEFF') json = json[1..];
        return new ModelCatalogParser(allowedSourceHosts, supportedBackendIds, allowedLicenseHosts).Parse(json);
    }
}
