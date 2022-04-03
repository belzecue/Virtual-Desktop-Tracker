using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using VDTracker.Properties;
using System.IO;

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
		private bool balloonTips = false; 
		private int vdNumber = 0, priorVDNumber = 0;
		private Guid currentVD;
		private int VDCheckInterval = 250;
		private string info, desktopName, balloonTipValue;
		private static IniFile iniFile;
		private string[] origDesktopSetting;
		private Timer balloonDelayTimer = new Timer() { Interval = 1000, Enabled = false };
		private System.ComponentModel.ComponentResourceManager resources;

		private class TestWindow : NewWindow
		{
			public TestWindow()
			{
				this.Size = Size.Empty;
				this.FormBorderStyle = FormBorderStyle.None;
			}
		}

		void OnProcessExit()
		{
			// Reset background to original wallpaper settings.
			Wallpaper.Set(0, iniFile);
		}

		public VDWindow()
		{
			InitializeComponent();

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

		//private void VDWindow_Reveal(object sender, EventArgs e)
		//{
		//	this.WindowState = FormWindowState.Normal;

		//}


		private void VDWindow_Notifications(object sender, EventArgs e)
		{
			// Toggle balloonTip notifications on/off.

			balloonTipValue = iniFile.Read("balloonTips", "Application").ToLower();
			if (balloonTipValue == "false" || balloonTipValue == "0" || balloonTipValue == "no") 
			{
				balloonTipValue = "true";
				balloonTips = true; 
			}
			else if (balloonTipValue == "true" || balloonTipValue == "1" || balloonTipValue == "yes")
			{
				balloonTipValue = "false";
				balloonTips = false; 
			}
			iniFile.Write("balloonTips", balloonTipValue, "Application");
		}


		private void VDWindow_PickImage(object sender, EventArgs e)
		{
			// Early out for vd0.  We never alter the original desktop wallpaper property.
			if (vdNumber == 0) { return; }

			string result = GetImagePath();
			if (result != string.Empty)
            {
				iniFile.Write("wallpaper", result, string.Concat("VD", vdNumber));
				Wallpaper.Set(vdNumber, iniFile);
			}
		}

			private void VDWindow_NameDesktop(object sender, EventArgs e)
		{
			// Early out for vd0.
			if (vdNumber == 0) { return; }

			string newName = String.Empty;
			//Display the custom input dialog box with the following prompt, window title, and dimensions
			DialogResult result = ShowInputDialogBox(ref newName, "Name this desktop:", "Virtual Desktop Tracker", 200, 100);
			if (result == DialogResult.OK && newName != String.Empty)
            {
				desktopName = newName;
				iniFile.Write("desktopName", newName, string.Concat("VD", vdNumber));

				// Update icon tooltip.
				info = string.Concat(
					vdNumber
					, (string.IsNullOrEmpty(desktopName)) ? string.Empty : string.Concat(" : ", desktopName)
				);
				notifyIcon.Text = info;
				this.Text = info;
			}
		}

		private static DialogResult ShowInputDialogBox(ref string input, string prompt, string title = "Title", int width = 300, int height = 200)
		{
			//This function creates the custom input dialog box by individually creating the different window elements and adding them to the dialog box

			//Specify the size of the window using the parameters passed
			Size size = new Size(width, height);
			//Create a new form using a System.Windows Form
			using (Form inputBox = new Form())
            {
				// Screen Position
				inputBox.StartPosition = FormStartPosition.Manual;
				int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
				int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

				inputBox.Location = new Point(
					screenWidth - width - (screenWidth / 10),
					screenHeight - height - (screenHeight / 10)
				);

				inputBox.FormBorderStyle = FormBorderStyle.Fixed3D;
				inputBox.ClientSize = size;
				//Set the window title using the parameter passed
				inputBox.Text = title;

				//Create a new label to hold the prompt
				Label label = new Label();
				label.Text = prompt;
				label.Location = new Point(5, 5);
				label.Width = size.Width - 10;
				inputBox.Controls.Add(label);

				//Create a textbox to accept the user's input
				TextBox textBox = new TextBox();
				textBox.Size = new Size(size.Width - 10, 25);
				textBox.Location = new Point(5, label.Location.Y + 20);
				textBox.Text = input;
				inputBox.Controls.Add(textBox);

				//Create an OK Button 
				Button okButton = new Button();
				okButton.DialogResult = DialogResult.OK;
				okButton.Name = "okButton";
				okButton.Size = new Size(75, 23);
				okButton.Text = "&OK";
				okButton.Location = new Point(size.Width - 80 - 80, size.Height - 30);
				inputBox.Controls.Add(okButton);

				//Create a Cancel Button
				Button cancelButton = new Button();
				cancelButton.DialogResult = DialogResult.Cancel;
				cancelButton.Name = "cancelButton";
				cancelButton.Size = new Size(75, 23);
				cancelButton.Text = "&Cancel";
				cancelButton.Location = new Point(size.Width - 80, size.Height - 30);
				inputBox.Controls.Add(cancelButton);

				//Set the input box's buttons to the created OK and Cancel Buttons respectively so the window appropriately behaves with the button clicks
				inputBox.AcceptButton = okButton;
				inputBox.CancelButton = cancelButton;

				//Show the window dialog box 
				DialogResult result = inputBox.ShowDialog();
				if (result == DialogResult.OK) { input = textBox.Text; }

				//After input has been submitted, return the input value
				return result;
			}
		}

		private string GetImagePath()
        {
			string filePath = string.Empty;
			string mruPicPath = iniFile.Read("picPath", "MRU");

			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = string.IsNullOrEmpty(mruPicPath) ? "C:\\" : mruPicPath;
				openFileDialog.Filter = "png (*.png)|*.png|jpg (*.jpg)|*.jpg|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 3;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					filePath = openFileDialog.FileName;
					iniFile.Write("picPath", Path.GetDirectoryName(filePath), "MRU");
				}
			}

			return filePath;
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

					// add new VDM Guid to list, if not existing
					currentVD = vdm.GetWindowDesktopId(this.Handle);
					if (!vdmList.ContainsKey(currentVD)) vdmList.Add(currentVD, vdmList.Count + 1);

					// update icon display
					if (
							vdmList.TryGetValue(currentVD, out vdNumber)
							&& vdNumber != priorVDNumber
						)
					{
						priorVDNumber = vdNumber;
						notifyIcon.Visible = false;
						balloonDelayTimer.Stop();

						this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject(string.Concat("notifyIcon", vdNumber, ".Icon"))));
						desktopName = iniFile.Read("desktopName", string.Concat("VD", vdNumber));
						balloonTipValue = iniFile.Read("balloonTips", "Application").ToLower();
						balloonTips = (balloonTipValue == "false" || balloonTipValue == "0" || balloonTipValue == "no") ? false: true;
						info = string.Concat(
							vdNumber
							, (string.IsNullOrEmpty(desktopName)) ? string.Empty : string.Concat(" : ", desktopName)
						);
						notifyIcon.Text = info;
						this.Text = info;

						// Update background image
						Wallpaper.Set(vdNumber, iniFile);

						// start delay timer
						if (balloonTips) balloonDelayTimer.Start();

						notifyIcon.Visible = true;
					}
				}
			}
			catch
			{
				//This will fail due to race conditions as currently written on occassion
				Console.WriteLine("VDCheckTimer_Tick failed due to race condition");
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
			OnProcessExit();
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
			this.VDCheckTimer.Interval = VDCheckInterval;
			this.VDCheckTimer.Tick += new System.EventHandler(this.VDCheckTimer_Tick);
			this.VDCheckTimer.Enabled = true;
			// 
			// VDWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(500, 0);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "VDWindow";
			this.Text = string.Concat("Desktop: ", vdNumber);
			this.TopMost = true;
			this.Load += new System.EventHandler(this.VDWindow_Load);
			this.ResumeLayout(false);

			//
			// NotifyIcon
			//
			balloonDelayTimer.Tick += new EventHandler(BalloonDelayTimer_Tick);
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));

			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon0.Icon")));

			//
			// Context menu
			//
			menu = new ContextMenu();
			menu.MenuItems.Add(0,
				new MenuItem("Name desktop", new System.EventHandler(this.VDWindow_NameDesktop))
			);
			menu.MenuItems.Add(1,
				new MenuItem("Choose pic", new System.EventHandler(this.VDWindow_PickImage))
			);
			menu.MenuItems.Add(2,
				new MenuItem(
					"Toggle Notification",
					new System.EventHandler(this.VDWindow_Notifications)
				)
			);
			menu.MenuItems.Add(3,
				new MenuItem("Exit", new System.EventHandler(this.VDWindow_Exit))
			);
			//menu.MenuItems.Add(2,
			//	new MenuItem("Show", new System.EventHandler(this.VDWindow_Reveal))
			//);
			notifyIcon.ContextMenu = menu;
		}

		private void BalloonDelayTimer_Tick(object sender, EventArgs e)
		{
			balloonDelayTimer.Stop();
			// show a notification
			notifyIcon.Visible = false;
			notifyIcon.Visible = true;
			this.notifyIcon.ShowBalloonTip(
				1
				, ""
				, string.Concat("On Desktop ", info)
				, ToolTipIcon.Info
			);
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
					origDesktopSetting = Wallpaper.GetOrigDesktopSettings();

					// record current desktop wallpaper settings
					iniFile.Write("wallpaperStyle", origDesktopSetting[0], "VD0");
					iniFile.Write("tileWallpaper", origDesktopSetting[1], "VD0");
					iniFile.Write("wallpaper", origDesktopSetting[2], "VD0");

					if (iniFile.Read("fileVersion", "Application") == string.Empty)
					{
						// no ini file yet, so create a default one

						iniFile.Write(
							"wallpaperStyles"
							, string.Concat(
								"Stretched:2, ",
								"Centered:0, ",
								"Tiled:0, ",
								"Fill:10, ",
								"Fit:6"
							)
							, "Help"
						);
						iniFile.Write(
							"tileWallpaper"
							, string.Concat(
								"Tiled:1, ",
								"Not Tiled:0"
							)
							, "Help"
						);

						iniFile.Write(
							"fileVersion"
							, FileVersionInfo.GetVersionInfo(iniFile.exeName).FileVersion.ToString()
							, "Application"
						);

						iniFile.Write(
							"balloonTips"
							, "true"
							, "Application"
						);

						for (int i = 1; i <= 9; i++)
						{
							string vd = string.Concat("VD", i);
							iniFile.Write("wallpaperStyle", origDesktopSetting[0], vd);
							iniFile.Write("tileWallpaper", origDesktopSetting[1], vd);
							iniFile.Write("wallpaper", origDesktopSetting[2], vd);
						}
					}
					else
					{
						// update fileVersion
						iniFile.Write(
							"fileVersion"
							, FileVersionInfo.GetVersionInfo(iniFile.exeName).FileVersion.ToString()
							, "Application"
						);
					}
					return string.Empty;
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			}
			else return string.Empty;
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
			if (Settings.Default.debug == true) { Console.WriteLine(TopLevelWindow); }
			if ((hr = manager.GetWindowDesktopId(TopLevelWindow, out result)) != 0)
			{
				if (Settings.Default.debug == true)
				{
					Console.WriteLine(result);
					Console.WriteLine("GetWindowDesktopId is source");
				}
				Marshal.ThrowExceptionForHR(hr);
			}
			return result;
		}

		public void MoveWindowToDesktop(IntPtr TopLevelWindow, Guid CurrentDesktop)
		{
			int hr;
			if ((hr = manager.MoveWindowToDesktop(TopLevelWindow, CurrentDesktop)) != 0)
			{
				if (Settings.Default.debug == true)	{ Console.WriteLine("MoveWindowToDesktop is source");	}
				Marshal.ThrowExceptionForHR(hr);
			}
		}
	}
}