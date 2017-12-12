using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace VDTracker
{
	/// <summary>
	/// Example form
	/// </summary>
	public partial class VDWindow : Form
	{
		private Dictionary<Guid, int> vdmList = new Dictionary<Guid, int>();
		private NotifyIcon notifyIcon;
		private ContextMenu menu;
		private int vdNumber = 0, priorVDNumber = 0;
		private Guid currentVD;
		private int VDCheckInterval = 250;
		private string info;
		private IniFile iniFile;
		private string[] origDesktopSetting;
		private System.ComponentModel.ComponentResourceManager resources;

		private class TestWindow : NewWindow
		{
			public TestWindow()
			{
				this.Size = Size.Empty;
				this.FormBorderStyle = FormBorderStyle.None;
			}
		}

		public VDWindow()
		{
			InitializeComponent();
			//BackColor = Color.Magenta;
			//TransparencyKey = Color.Magenta;

			// Initialize INI file
			string result;
			if ((result = InitializeINIFile()) != string.Empty)
			{
				Console.WriteLine(string.Concat("Failed to initialize INI file: ", result));
				Application.Exit();
			}
		}

		private VirtualDesktopManager vdm;
		private void VDWindow_Load(object sender, EventArgs e)
		{
			//Create IVirtualDesktopManager on load
			vdm = new VirtualDesktopManager();

			// minimize window to tray
			notifyIcon.Visible = true;
			this.WindowState = FormWindowState.Minimized;
			this.ShowInTaskbar = false;
		}

		private void VDWindow_Exit(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void VDWindow_Reveal(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Normal;

		}

		//Timer tick to check if the window is on the current virtual desktop and change it otherwise
		//A timer does not have to be used, but something has to trigger the check
		//If the window was active before the vd change, it would trigger 
		//the deactivated and lost focus events when the vd changes
		//The timer always gets triggered which makes the example hopefully less confusing
		private void VDCheckTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				// move window to current VD
				if (!vdm.IsWindowOnCurrentVirtualDesktop(Handle))
				{
					using (TestWindow nw = new TestWindow())
					{
						nw.Show(null);
						vdm.MoveWindowToDesktop(Handle, vdm.GetWindowDesktopId(nw.Handle));
					}

					//Console.WriteLine(string.Concat("Switching...", vdmList.Count));

					// add new VDM Guid to list, if not existing
					currentVD = vdm.GetWindowDesktopId(this.Handle);
					if (!vdmList.ContainsKey(currentVD)) vdmList.Add(currentVD, vdmList.Count + 1);

					// update icon display
					if (
							vdmList.TryGetValue(currentVD, out vdNumber)
							&& vdNumber != priorVDNumber
						)
					{
						this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject(string.Concat("notifyIcon", vdNumber, ".Icon"))));
						info = string.Concat("VD: ", vdNumber);
						notifyIcon.Text = info;
						this.Text = info;
						priorVDNumber = vdNumber;

						// Update background image
						//Wallpaper.Set(
						//	new System.Uri(iniFile.Read("image", string.Concat("VD", vdNumber)))
						//	, Wallpaper.Style.Fill
						//);
					}
				}
			}
			catch
			{
				//This will fail due to race conditions as currently written on occassion
				Console.WriteLine("Failed due to race condition");
			}
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.VDCheckTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// VDCheckTimer
			// 
			this.VDCheckTimer.Enabled = true;
			this.VDCheckTimer.Interval = VDCheckInterval;
			this.VDCheckTimer.Tick += new System.EventHandler(this.VDCheckTimer_Tick);
			// 
			// VDWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(500, 0);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "VDWindow";
			this.Text = string.Concat("VD: ", vdNumber);
			this.TopMost = true;
			this.Load += new System.EventHandler(this.VDWindow_Load);
			this.ResumeLayout(false);

			//
			// NotifyIcon
			//
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));

			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon0.Icon")));

			//
			// Context menu
			//
			menu = new ContextMenu();
			menu.MenuItems.Add(0,
				new MenuItem("Exit", new System.EventHandler(this.VDWindow_Exit))
			);
			menu.MenuItems.Add(1,
				new MenuItem("Show", new System.EventHandler(this.VDWindow_Reveal))
			);
			notifyIcon.ContextMenu = menu;
		}

		#endregion

		private System.Windows.Forms.Timer VDCheckTimer;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new VDWindow());
		}

		private string InitializeINIFile()
		{
			if (iniFile == null)
			{
				try
				{
					iniFile = new IniFile();
					origDesktopSetting = Wallpaper.GetDesktopSettings();

					// record current desktop wallpaper settings
					iniFile.Write(
						"oldWallpaperStyle"
						, origDesktopSetting[0]
						, "Application"
					);
					iniFile.Write(
						"oldTileWallpaper"
						, origDesktopSetting[1]
						, "Application"
					);
					iniFile.Write(
						"oldWallpaper"
						, origDesktopSetting[2]
						, "Application"
					);

					if (iniFile.Read("fileVersion", "Application") == string.Empty)
					{
						// no ini file yet, so create a default one
						iniFile.Write(
							"fileVersion"
							, FileVersionInfo.GetVersionInfo(iniFile.exeName).FileVersion.ToString()
							, "Application"
						);

						for (int i = 1; i <= 9; i++)
						{
							string vd = string.Concat("VD", i);
							iniFile.Write("wallpaperStyle", origDesktopSetting[0], vd);
							iniFile.Write("tileWallpaper", origDesktopSetting[1], vd);
							iniFile.Write("wallpaper", ConvertPathToURI(origDesktopSetting[2]), vd);
						}
					}

					// update current background storage


					return string.Empty;
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			}
			else return string.Empty;
		}

		private string ConvertPathToURI(string path)
		{
			return string.Concat(
				@"file:///"
				, path.Replace(@"\", "/")
		   );
		}
	}
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
	[System.Security.SuppressUnmanagedCodeSecurity]
	public interface IVirtualDesktopManager
	{
		[PreserveSig]
		int IsWindowOnCurrentVirtualDesktop(
			[In] IntPtr TopLevelWindow,
			[Out] out int OnCurrentDesktop
			);
		[PreserveSig]
		int GetWindowDesktopId(
			[In] IntPtr TopLevelWindow,
			[Out] out Guid CurrentDesktop
			);

		[PreserveSig]
		int MoveWindowToDesktop(
			[In] IntPtr TopLevelWindow,
			[MarshalAs(UnmanagedType.LPStruct)]
			[In]Guid CurrentDesktop
			);
	}

	public class NewWindow : Form
	{
	}
	[ComImport, Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")]
	public class CVirtualDesktopManager
	{

	}
	public class VirtualDesktopManager
	{
		public VirtualDesktopManager()
		{
			cmanager = new CVirtualDesktopManager();
			manager = (IVirtualDesktopManager)cmanager;
		}
		~VirtualDesktopManager()
		{
			manager = null;
			cmanager = null;
		}
		private CVirtualDesktopManager cmanager = null;
		private IVirtualDesktopManager manager;

		public bool IsWindowOnCurrentVirtualDesktop(IntPtr TopLevelWindow)
		{
			int result;
			int hr;
			if ((hr = manager.IsWindowOnCurrentVirtualDesktop(TopLevelWindow, out result)) != 0)
			{
				Marshal.ThrowExceptionForHR(hr);
			}
			return result != 0;
		}

		public Guid GetWindowDesktopId(IntPtr TopLevelWindow)
		{
			Guid result;
			int hr;
			if ((hr = manager.GetWindowDesktopId(TopLevelWindow, out result)) != 0)
			{
				Marshal.ThrowExceptionForHR(hr);
			}
			return result;
		}

		public void MoveWindowToDesktop(IntPtr TopLevelWindow, Guid CurrentDesktop)
		{
			int hr;
			if ((hr = manager.MoveWindowToDesktop(TopLevelWindow, CurrentDesktop)) != 0)
			{
				Marshal.ThrowExceptionForHR(hr);
			}
		}
	}
}