using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MoreConvenientJiraSvn.Plugin
{
    public record PluginInfo
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Version { get; set; }
        public required string Author { get; set; }
        public ImageSource? Image
        {
            get
            {
                if (!string.IsNullOrEmpty(ImageUrl))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(ImageUrl, UriKind.Absolute);
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
                return null;
            }
        }
        public string? ImageUrl { get; set; }
        public DateTime UpdateTime { get; set; }
        public MD5? MD5 { get; set; }
    }
}
