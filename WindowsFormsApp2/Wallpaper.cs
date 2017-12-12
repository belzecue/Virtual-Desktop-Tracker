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

		public static void Set(int vDesktopNum, IniFile iniFile)
		{
			string vdString = string.Concat("VD", vDesktopNum);
			string tempPath;

			if (vDesktopNum == 0)
			{
				tempPath = iniFile.Read("wallpaper", vdString);
			}
			else
			{
				System.IO.Stream s = new System.Net.WebClient().OpenRead(
					iniFile.Read("wallpaper", vdString)
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

		public static string[] GetDesktopSettings()
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
