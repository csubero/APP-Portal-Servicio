using System;
using Android.Content;
using PortalServicio.Services;
using PortalServicio.Droid;
using Xamarin.Forms;
using System.IO;
using Java.IO;

[assembly: Dependency(typeof(SaveAndroid))]
namespace PortalServicio.Droid
{
    public class SaveAndroid : ISave
    {
        public byte[] GetDocumentBytesAsync(string fileName)
        {
            string root = null;
            if (Android.OS.Environment.IsExternalStorageEmulated)
            {
                root = Android.OS.Environment.ExternalStorageDirectory.ToString();
            }
            else
                root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            using (var streamReader = new StreamReader(root + "/recibos/"+fileName))
            {
                var bytes = default(byte[]);
                using (var memstream = new MemoryStream())
                {
                    streamReader.BaseStream.CopyTo(memstream);
                    bytes = memstream.ToArray();
                }
                return bytes;
            }
            
        }
        public bool SaveTextAsync(string fileName, String contentType, MemoryStream s, bool open = true)
        //public async Task<bool> SaveTextAsync(string fileName, String contentType, MemoryStream s, bool open = true)
        {
            string root = null;
            if (Android.OS.Environment.IsExternalStorageEmulated)
            {
                root = Android.OS.Environment.ExternalStorageDirectory.ToString();
            }
            else
                root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

            Java.IO.File myDir = new Java.IO.File(root + "/recibos");
            myDir.Mkdir();

            Java.IO.File file = new Java.IO.File(myDir, fileName);

            if (file.Exists()) file.Delete();
                FileOutputStream outs = new FileOutputStream(file);
                outs.Write(s.ToArray());
                outs.Flush();
                outs.Close();
            if (file.Exists() & open)
            {
                Android.Net.Uri path = Android.Net.Uri.FromFile(file);
                string extension = Android.Webkit.MimeTypeMap.GetFileExtensionFromUrl(Android.Net.Uri.FromFile(file).ToString());
                string mimeType = Android.Webkit.MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(path, mimeType);
#pragma warning disable CS0618 // Type or member is obsolete
                Forms.Context.StartActivity(Intent.CreateChooser(intent, "Choose App"));
#pragma warning restore CS0618 // Type or member is obsolete
            }
            return true;
        }
    }
}
