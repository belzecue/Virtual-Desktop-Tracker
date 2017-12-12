using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace VDTracker
{
	class IniFile   // revision 11
	{
		public string path = AppDomain.CurrentDomain.BaseDirectory;
		public string exeName = System.Reflection.Assembly.GetEntryAssembly().Location;

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

		public IniFile()
		{
			path = new FileInfo(Path.Combine(path, exeName + ".ini")).FullName.ToString();
		}

		public string Read(string Key, string Section = null)
		{
			var RetVal = new StringBuilder(255);
			GetPrivateProfileString(Section ?? exeName, Key, "", RetVal, 255, path);
			return RetVal.ToString();
		}

		public void Write(string Key, string Value, string Section = null)
		{
			WritePrivateProfileString(Section ?? exeName, Key, Value, path);
		}

		public void DeleteKey(string Key, string Section = null)
		{
			Write(Key, null, Section ?? exeName);
		}

		public void DeleteSection(string Section = null)
		{
			Write(null, null, Section ?? exeName);
		}

		public bool KeyExists(string Key, string Section = null)
		{
			return Read(Key, Section).Length > 0;
		}
	}
}
