using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;


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
			string tempPath;

			string wallpaperPath = iniFile.Read("wallpaper", vdString);
			if (wallpaperPath == @"file:///") { wallpaperPath = string.Empty; }

			if (vDesktopNum == 0 || wallpaperPath == string.Empty)
			{
				tempPath = wallpaperPath;
			}
			else
			{
				System.IO.Stream s = new System.Net.WebClient().OpenRead(
					wallpaperPath
				);
				System.Drawing.Image img = System.Drawing.Image.FromStream(s);
				tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.png");
				img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
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
			result[2] = key.GetValue("Wallpaper").ToString();

			return result;
		}
	}
}
