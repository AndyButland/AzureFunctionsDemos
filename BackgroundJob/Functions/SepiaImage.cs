namespace BackgroundJob.Functions
{
    using System.Drawing;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Function that creates a sepia version of an image uploaded to blob storage.
    /// </summary>
    /// <remarks>
    /// Hat-tip for sepia image generation code: https://www.dyclassroom.com/csharp-project/how-to-convert-a-color-image-into-sepia-image-in-csharp-using-visual-studio
    /// </remarks>
    public static class SepiaImage
    {
        [FunctionName("SepiaImage")]
        public static async Task Run(
            [BlobTrigger("image-uploads/{name}", Connection = "AzureWebJobsStorage")]Stream uploadedImage, 
            string name, 
            [Blob("image-uploads-sepia/{name}", Connection = "AzureWebJobsStorage")]CloudBlockBlob sepiaImage,
            TraceWriter log)
        {
            // Create sepia version of the image as a stream
            var sepiaImageStream = GetSepiaImage(uploadedImage);
            sepiaImageStream.Seek(0, SeekOrigin.Begin);

            // Set appropriate content type for created image
            sepiaImage.Properties.ContentType = "image/jpeg";

            // Populate the new image from the stream
            await sepiaImage.UploadFromStreamAsync(sepiaImageStream);
        }

        private static Stream GetSepiaImage(Stream originalImage)
        {
            var bmp = new Bitmap(originalImage);

            for (var y = 0; y < bmp.Height; y++)
            {
                for (var x = 0; x < bmp.Width; x++)
                {
                    var p = bmp.GetPixel(x, y);

                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    var tr = (int)(0.393 * r + 0.769 * g + 0.189 * b);
                    var tg = (int)(0.349 * r + 0.686 * g + 0.168 * b);
                    var tb = (int)(0.272 * r + 0.534 * g + 0.131 * b);

                    r = tr > 255 ? 255 : tr;
                    g = tg > 255 ? 255 : tg;
                    b = tb > 255 ? 255 : tb;

                    bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            var memoryStream = new MemoryStream();
            bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return memoryStream;
        }
    }
}
