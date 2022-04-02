using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;


namespace VDTracker
{
	public sealed class Wallpaper
	{
		// See this reference code from 2011: https://gist.github.com/belzecue/5b2be19151567ea11761cefb3aae357a

		Wallpaper() { }

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
		const int SPI_SETDESKWALLPAPER = 20;
		const int SPIF_UPDATEINIFILE = 0x01;
		const int SPIF_SENDWININICHANGE = 0x02;

		public enum Style : int
		{
			Tiled,
			Centered,
			Stretched,
			Fill,
			Fit
		}

		public static void Set(int vDesktopNum, IniFile iniFile)
		{
			string vdString = string.Concat("VD", vDesktopNum);
			string tempPath = @Path.Combine(Path.GetTempPath(), "vdtracker_wallpaper.png");
			string wallpaperPath = @iniFile.Read("wallpaper", vdString);

			if (vDesktopNum > 0 && File.Exists(wallpaperPath))
			{
				using (Image image = Image.FromFile(wallpaperPath))
                {
					image.Save(tempPath, ImageFormat.Png);
				}
			}
			else
            {
				tempPath = wallpaperPath;
			}

			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			key.SetValue(@"WallpaperStyle", iniFile.Read("wallpaperStyle", vdString));
			key.SetValue(@"TileWallpaper", iniFile.Read("tileWallpaper", vdString));

			SystemParametersInfo(SPI_SETDESKWALLPAPER,
				0,
				tempPath,
				SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
		}

		public static string[] GetOrigDesktopSettings()
		{
			string[] result = new string[3];
			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			result[0] = key.GetValue("WallpaperStyle").ToString();
			result[1] = key.GetValue("TileWallpaper").ToString();
			result[2] = @key.GetValue("Wallpaper").ToString();

			return result;
		}
	}
}
