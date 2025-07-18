using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

public class App : Form
{
    private TextBox logText;
    private Process tlProcess;
    private NotifyIcon notifyIcon;
    private int maxLogCount = 500;

    public App()
    {
        this.Text = "Twlight-Line-Windows-GUI";
        this.Size = new Size(800, 600);
        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Icon = new Icon(Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("twilight_line.ico"));

        this.logText = new TextBox();
        this.logText.BorderStyle = BorderStyle.None;
        this.logText.Multiline = true;
        this.logText.ReadOnly = true;
        this.logText.Size = this.ClientSize;
        this.Controls.Add(this.logText);

        this.notifyIcon = new NotifyIcon();
        this.notifyIcon.Icon = this.Icon;
        this.notifyIcon.Text = this.Text;
        this.notifyIcon.DoubleClick += this.OnNotifyIconDoubleClick;
        this.notifyIcon.Visible = true;
        this.notifyIcon.ContextMenu = new ContextMenu();
        {
            MenuItem item = new MenuItem("Show");
            item.Click += this.OnNotifyIconMenuShow;
            this.notifyIcon.ContextMenu.MenuItems.Add(item);
        }
        {
            MenuItem item = new MenuItem("Hide");
            item.Click += this.OnNotifyIconMenuHide;
            this.notifyIcon.ContextMenu.MenuItems.Add(item);
        }
        {
            MenuItem item = new MenuItem("Exit");
            item.Click += this.OnNotifyIconMenuExit;
            this.notifyIcon.ContextMenu.MenuItems.Add(item);
        }

        this.Load += this.OnLoad;
        this.Shown += this.OnShown;
        this.FormClosed += this.OnClose;
        this.Resize += this.OnResize;
    }

    public void OnLoad(object sender, EventArgs args)
    {
        try {
            string workDir = Path.GetFullPath(".\\");
            Process p = new Process();
            p.StartInfo.FileName = workDir +
                "twilight-line-client.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.Arguments =
                "-e \"" + workDir + "config.json\"";
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WorkingDirectory = workDir;
            p.OutputDataReceived += this.OnChildProcessPrint;
            p.ErrorDataReceived += this.OnChildProcessPrint;
            this.tlProcess = p;

            this.tlProcess.Start();
            this.tlProcess.BeginOutputReadLine();
            this.tlProcess.BeginErrorReadLine();

        } catch (Exception e) {
            MessageBox.Show(e.Message);
            Environment.Exit(1);
        }
    }

    public void OnShown(object sender, EventArgs args)
    {
        MinToTray();
    }

    public void OnClose(object sender, EventArgs args)
    {
        try {
            if (this.tlProcess != null) {
                this.tlProcess.CancelErrorRead();
                this.tlProcess.CancelOutputRead();
                this.tlProcess.Kill();
                this.tlProcess.Close();
                this.tlProcess = null;
            }
        } catch {}

        this.notifyIcon.Visible = false;
    }

    public void OnResize(object sender, EventArgs args)
    {
        if (this.WindowState == FormWindowState.Minimized) {
            this.MinToTray();
        }
    }

    public void OnNotifyIconDoubleClick(object sender, EventArgs args)
    {
        if (this.Visible) {
            this.MinToTray();
        } else {
            this.MaxFromTray();
        }
    }

    public void OnNotifyIconMenuShow(object sender, EventArgs args)
    {
        this.MaxFromTray();
    }

    public void OnNotifyIconMenuHide(object sender, EventArgs args)
    {
        this.MinToTray();
    }

    public void OnNotifyIconMenuExit(object sender, EventArgs args)
    {
        Application.Exit();
    }

    public void OnChildProcessPrint(object sender, DataReceivedEventArgs data)
    {
        if (this.logText.Lines.Length >= this.maxLogCount) {
            this.logText.Text = "";
        }
        this.logText.AppendText(data.Data + "\r\n");
        this.logText.Update();
    }

    public void MinToTray()
    {
        this.Hide();
    }

    public void MaxFromTray()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
    }

    public static void Main()
    {
        Application.Run(new App());
    }
}
