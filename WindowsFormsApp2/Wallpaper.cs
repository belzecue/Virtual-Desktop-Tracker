using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace VDTracker
{
	public sealed class Wallpaper
	{
		Wallpaper() { }

		const int SPI_SETDESKWALLPAPER = 20;
		const int SPIF_UPDATEINIFILE = 0x01;
		const int SPIF_SENDWININICHANGE = 0x02;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

		public enum Style : int
		{
			Tiled,
			Centered,
			Stretched,
			Fill,
			Fit
		}

		public static void Set(Uri uri, Style style)
		{
			System.IO.Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

			System.Drawing.Image img = System.Drawing.Image.FromStream(s);
			string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.png");
			img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			if (style == Style.Stretched)
			{
				key.SetValue(@"WallpaperStyle", 2.ToString());
				key.SetValue(@"TileWallpaper", 0.ToString());
			}

			if (style == Style.Centered)
			{
				key.SetValue(@"WallpaperStyle", 0.ToString());
				key.SetValue(@"TileWallpaper", 0.ToString());
			}

			if (style == Style.Tiled)
			{
				key.SetValue(@"WallpaperStyle", 0.ToString());
				key.SetValue(@"TileWallpaper", 1.ToString());
			}

			if (style == Style.Fill)
			{
				key.SetValue(@"WallpaperStyle", 10.ToString());
				key.SetValue(@"TileWallpaper", 0.ToString());
			}

			if (style == Style.Fit)
			{
				key.SetValue(@"WallpaperStyle", 6.ToString());
				key.SetValue(@"TileWallpaper", 0.ToString());
			}

			SystemParametersInfo(SPI_SETDESKWALLPAPER,
				0,
				tempPath,
				SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
		}

		public static string[] GetDesktopSettings()
		{
			string[] result = new string[2];
			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			result[0] = key.GetValue("WallpaperStyle").ToString();
			result[1] = key.GetValue("TileWallpaper").ToString();
			return result;
		}
	}
}
