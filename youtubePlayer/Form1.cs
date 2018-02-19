using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IniManager;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Security;
using System.Runtime.InteropServices;

namespace youtubePlayer
{
    public partial class Form1 : Form
    {




        public static Form mainFrm;
        public static double preOpacity;
        public static bool isLoading = true;

        iniClass iniCls = new iniClass();

        string iniPath;
        string iniFile;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void MessageForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }



        public Form1()
        {
            InitializeComponent();
           // this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;



            BrowserSet BrowserSetting = new BrowserSet();

            if (!BrowserSetting.IsBrowserEmulationSet())
            {
                BrowserSetting.SetBrowserEmulationVersion();
            }

            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            mainFrm = this;
            webBrowser1.Visible = false;

            webBrowser1.Navigate("about:blank");//회색 박스에 드래그&드랍
            //string htmlSrc = @"<html><body style='margin:0px;padding:0px;'><div style='margin:0px;padding:0px;'><embed src='http://www.youtube.com/v/{0}?version=3&amp;hl=ko_KR&amp;vq=hd720&autoplay=1' type='application/x-shockwave-flash' width='100%' height='100%' ='always' allowfullscreen='true'></embed></div></body></html>";
            //webBrowser1.Document.Write(src);

            dataGridView1.Columns.Add("Title", "Title");

            iniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "youtubeplayer");
            iniFile = Path.Combine(iniPath, "setting.ini");

            if (Directory.Exists(iniPath) == false) Directory.CreateDirectory(iniPath);
            
            string urls = iniCls.GetIniValue("last", "urls", iniFile);
            string[] urlsArr;
            if (urls != "")
            {
                urlsArr = urls.Split(',');

                for (int i = 0; i < urlsArr.Length; i++)
                {
                    dataGridView1.Rows.Add(urlsArr[i]);
                }
            }

            MouseHook.Start();
        }
        protected override CreateParams CreateParams // alt+tab에 표시 없애기
        {
            get
            {
                CreateParams pm = base.CreateParams;
                pm.ExStyle |= 0x80;
                return pm;
            }
        }
        
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.Opacity = (double)trackBar1.Value / 10d;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MouseHook.stop();
            string urls = "";
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    urls += dataGridView1.Rows[i].Cells[0].Value.ToString() + ",";
                }
                urls = urls.Substring(0, urls.Length - 1);
                iniCls.SetIniValue("last", "urls", urls, iniFile);
            }
            else
            {
                iniCls.SetIniValue("last", "urls", "", iniFile);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("시작할 재생 목록 선택하세요.","youtube palyer");
                return;
            }

            isLoading = true;

            MouseHook.stop();
            
            button_hide_Click(null, null);

            webBrowser1.Visible = true;
            webBrowser1.Dock = DockStyle.Fill;

            string htmlSrc = "<iframe width=\"100%\" height=\"100%\" src=\"https://www.youtube.com/embed/{0}\" frameborder=\"0\" allow=\"autoplay; encrypted-media\" allowfullscreen></iframe>";
            string url = "";
           // string url2 = "&playlist=";
            string src = "";

            if (dataGridView1.Rows.Count == 1)
            {
                src = string.Format(htmlSrc, dataGridView1.Rows[0].Cells[0].Value.ToString(), "");
            }
            else
            {
                url = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();

                src = string.Format(htmlSrc, url);
            }
            
            webBrowser1.Document.Write(src);
            webBrowser1.Refresh();

            System.Diagnostics.Debug.WriteLine("url : " + src);

            isLoading = false;
            timer1.Start();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            isLoading = false;
            dataGridView1.ClearSelection();
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.Data.GetData(DataFormats.Text);
                if (text.Contains("v="))
                {
                    dataGridView1.Rows.Add(text.Split('=')[1].ToString());
                }
            }
        }

        private void button_hide_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
            if (button_hide.Text == "<") button_hide.Text = ">";
            else button_hide.Text = "<";
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                for(int x =0 ; x < dataGridView1.SelectedRows.Count ; x++)
                {
                    dataGridView1.Rows.Remove(dataGridView1.SelectedRows[x]);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            MouseHook.Start();
        }

        private void TitlebarBtn_Click(object sender, EventArgs e)
        {
            if(this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None)
            {
                TitlebarBtn.Text = "Hide Title Bar";
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            }
            else
            {
                TitlebarBtn.Text = "Show Title Bar";
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            }
            
        }
    }

    public class BrowserSet
    {
        private const string InternetExplorerRootKey = @"Software\Microsoft\Internet Explorer";

        public int GetInternetExplorerMajorVersion()
        {
            int result;

            result = 0;

            try
            {
                RegistryKey key;

                key = Registry.LocalMachine.OpenSubKey(InternetExplorerRootKey);

                if (key != null)
                {
                    object value;

                    value = key.GetValue("svcVersion", null) ?? key.GetValue("Version", null);

                    if (value != null)
                    {
                        string version;
                        int separator;

                        version = value.ToString();
                        separator = version.IndexOf('.');
                        if (separator != -1)
                        {
                            int.TryParse(version.Substring(0, separator), out result);
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                // The user does not have the permissions required to read from the registry key.
            }
            catch (UnauthorizedAccessException)
            {
                // The user does not have the necessary registry rights.
            }

            return result;
        }
        public enum BrowserEmulationVersion
        {
            Default = 0,
            Version7 = 7000,
            Version8 = 8000,
            Version8Standards = 8888,
            Version9 = 9000,
            Version9Standards = 9999,
            Version10 = 10000,
            Version10Standards = 10001,
            Version11 = 11000,
            Version11Edge = 11001
        }
        private const string BrowserEmulationKey = InternetExplorerRootKey + @"\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

        public BrowserEmulationVersion GetBrowserEmulationVersion()
        {
            BrowserEmulationVersion result;

            result = BrowserEmulationVersion.Default;

            try
            {
                RegistryKey key;

                key = Registry.CurrentUser.OpenSubKey(BrowserEmulationKey, true);
                if (key != null)
                {
                    string programName;
                    object value;

                    programName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
                    value = key.GetValue(programName, null);

                    if (value != null)
                    {
                        result = (BrowserEmulationVersion)Convert.ToInt32(value);
                    }
                }
            }
            catch (SecurityException)
            {
                // The user does not have the permissions required to read from the registry key.
            }
            catch (UnauthorizedAccessException)
            {
                // The user does not have the necessary registry rights.
            }

            return result;
        }

        public bool IsBrowserEmulationSet()
        {
            return GetBrowserEmulationVersion() != BrowserEmulationVersion.Default;
        }
        public bool SetBrowserEmulationVersion(BrowserEmulationVersion browserEmulationVersion)
        {
            bool result;

            result = false;

            try
            {
                RegistryKey key;

                key = Registry.CurrentUser.OpenSubKey(BrowserEmulationKey, true);

                if (key != null)
                {
                    string programName;

                    programName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

                    if (browserEmulationVersion != BrowserEmulationVersion.Default)
                    {
                        // if it's a valid value, update or create the value
                        key.SetValue(programName, (int)browserEmulationVersion, RegistryValueKind.DWord);
                    }
                    else
                    {
                        // otherwise, remove the existing value
                        key.DeleteValue(programName, false);
                    }

                    result = true;
                }
            }
            catch (SecurityException)
            {
                // The user does not have the permissions required to read from the registry key.
            }
            catch (UnauthorizedAccessException)
            {
                // The user does not have the necessary registry rights.
            }

            return result;
        }

        public bool SetBrowserEmulationVersion()
        {
            int ieVersion;
            BrowserEmulationVersion emulationCode;

            ieVersion = GetInternetExplorerMajorVersion();

            if (ieVersion >= 11)
            {
                emulationCode = BrowserEmulationVersion.Version11;
            }
            else
            {
                switch (ieVersion)
                {
                    case 10:
                        emulationCode = BrowserEmulationVersion.Version10;
                        break;
                    case 9:
                        emulationCode = BrowserEmulationVersion.Version9;
                        break;
                    case 8:
                        emulationCode = BrowserEmulationVersion.Version8;
                        break;
                    default:
                        emulationCode = BrowserEmulationVersion.Version7;
                        break;
                }
            }

            return SetBrowserEmulationVersion(emulationCode);
        }
    }

}
