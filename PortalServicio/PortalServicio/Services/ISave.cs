using System.IO;

namespace PortalServicio.Services
{
    public interface ISave
    {
        bool SaveTextAsync(string filename, string contentType, MemoryStream s, bool open = true);
        byte[] GetDocumentBytesAsync(string filename);
    }
}
