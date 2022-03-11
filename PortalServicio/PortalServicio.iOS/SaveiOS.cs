using System;
using UIKit;
using Xamarin.Forms;
using PortalServicio.iOS;
using PortalServicio.Services;
using System.IO;
using QuickLook;
using System.Threading.Tasks;

[assembly: Dependency(typeof(SaveiOS))]
namespace PortalServicio.iOS
{
    class SaveiOS : ISave
    {
        public byte[] GetDocumentBytesAsync(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(path, filename);
            try
            {
                FileStream fileStream = File.Open(filePath, FileMode.Open);
                MemoryStream s = new MemoryStream();
                fileStream.CopyTo(s);
                fileStream.Close();
                return s.ToArray();
            }
            catch (Exception e)
            {
                throw new  Exception("Error al leer archivo.");
            }
        }

        public async Task<bool> SaveTextAsync(string filename, string contentType, MemoryStream s, bool open = true)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(path, filename);
            try
            {
                FileStream fileStream = File.Open(filePath, FileMode.Create);
                s.Position = 0;
                s.CopyTo(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
            catch (Exception e)
            {
                return false;
            }

            if (open)
            {
                UIViewController currentController = UIApplication.SharedApplication.KeyWindow.RootViewController;
                while (currentController.PresentedViewController != null)
                    currentController = currentController.PresentedViewController;
                UIView currentView = currentController.View;

                QLPreviewController qlPreview = new QLPreviewController();
                QLPreviewItem item = new QLPreviewItemBundle(filename, filePath);
                qlPreview.DataSource = new PreviewControllerDS(item);
                currentController.PresentViewController(qlPreview, true, null);
                while (!currentController.PresentedViewController.IsBeingDismissed) //retains execution until pdf viewer is closed.
                    await Task.Delay(250);            
            }
            return true;
        }

        bool ISave.SaveTextAsync(string filename, string contentType, MemoryStream s, bool open)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(path, filename);
            try
            {
                FileStream fileStream = File.Open(filePath, FileMode.Create);
                s.Position = 0;
                s.CopyTo(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
            catch (Exception e)
            {
                return false;
            }

            if (open)
            {
                UIViewController currentController = UIApplication.SharedApplication.KeyWindow.RootViewController;
                while (currentController.PresentedViewController != null)
                    currentController = currentController.PresentedViewController;
                UIView currentView = currentController.View;

                QLPreviewController qlPreview = new QLPreviewController();
                QLPreviewItem item = new QLPreviewItemBundle(filename, filePath);
                qlPreview.DataSource = new PreviewControllerDS(item);
                currentController.PresentViewController(qlPreview, true, null);
                while (!currentController.PresentedViewController.IsBeingDismissed) //retains execution until pdf viewer is closed.
                    Task.Delay(250).ConfigureAwait(true);
            }
            return true;
    }
    }
}
