using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;

namespace AntiSleepDAC
{
    public class SilentAudioPlayer
    {
        [DllImport("winmm.dll")]
        public static extern uint waveOutGetNumDevs();

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public struct WAVEOUTCAPS
        {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint dwFormats;
            public ushort wChannels;
            public ushort wReserved1;
            public uint dwSupport;
        }

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern uint waveOutGetDevCaps(uint uDeviceID, ref WAVEOUTCAPS pwoc, uint cbwoc);

        [StructLayout(LayoutKind.Sequential)]
        public struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public uint dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        [DllImport("winmm.dll")]
        public static extern uint waveOutOpen(out IntPtr hWaveOut, uint uDeviceID, ref WAVEFORMATEX lpFormat, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll")]
        public static extern uint waveOutPrepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize);

        [DllImport("winmm.dll")]
        public static extern uint waveOutWrite(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize);

        [DllImport("winmm.dll")]
        public static extern uint waveOutUnprepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize);

        [DllImport("winmm.dll")]
        public static extern uint waveOutClose(IntPtr hWaveOut);
        
        [DllImport("winmm.dll")]
        public static extern uint waveOutReset(IntPtr hWaveOut);

        public const ushort WAVE_FORMAT_PCM = 1;

        public static List<string> GetDevices()
        {
            List<string> devices = new List<string>();
            devices.Add("Výchozí zařízení systému (Základní)"); // Index 0 (WAVE_MAPPER)
            uint numDevs = waveOutGetNumDevs();
            for (uint i = 0; i < numDevs; i++)
            {
                WAVEOUTCAPS caps = new WAVEOUTCAPS();
                waveOutGetDevCaps(i, ref caps, (uint)Marshal.SizeOf(typeof(WAVEOUTCAPS)));
                devices.Add(caps.szPname);
            }
            return devices;
        }

        public static void PlaySilence(int deviceIndex)
        {
            // Device index 0 is Výchozí, which maps to -1 (WAVE_MAPPER in Windows API)
            uint uDeviceID = (deviceIndex == 0) ? unchecked((uint)-1) : (uint)(deviceIndex - 1);

            WAVEFORMATEX format = new WAVEFORMATEX();
            format.wFormatTag = WAVE_FORMAT_PCM;
            format.nChannels = 1;         // Mono
            format.nSamplesPerSec = 44100; // 44.1 kHz
            format.wBitsPerSample = 16;   // 16-bit
            format.nBlockAlign = (ushort)(format.nChannels * (format.wBitsPerSample / 8));
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;
            format.cbSize = 0;

            IntPtr hWaveOut = IntPtr.Zero;
            uint result = waveOutOpen(out hWaveOut, uDeviceID, ref format, IntPtr.Zero, IntPtr.Zero, 0);
            if (result != 0) return; // Failure silently handled

            int sampleRate = 44100;
            int numSamples = sampleRate * 1; // 1 second length
            byte[] silenceData = new byte[numSamples * 2]; // all 0 values (PCM silence)

            GCHandle bufferHandle = GCHandle.Alloc(silenceData, GCHandleType.Pinned);

            WAVEHDR hdr = new WAVEHDR();
            hdr.lpData = bufferHandle.AddrOfPinnedObject();
            hdr.dwBufferLength = (uint)silenceData.Length;
            hdr.dwFlags = 0;

            uint prepResult = waveOutPrepareHeader(hWaveOut, ref hdr, (uint)Marshal.SizeOf(typeof(WAVEHDR)));
            if (prepResult == 0)
            {
                waveOutWrite(hWaveOut, ref hdr, (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                Thread.Sleep(1100); // Cekani na dohrati jedne sekundy audia
            }

            waveOutReset(hWaveOut);
            waveOutUnprepareHeader(hWaveOut, ref hdr, (uint)Marshal.SizeOf(typeof(WAVEHDR)));
            waveOutClose(hWaveOut);

            bufferHandle.Free();
        }
    }

    public class MainForm : Form
    {
        ComboBox cmbDevices;
        NumericUpDown numInterval;
        Button btnSave;
        Button btnSaveAndRun;
        Label lblInfo;

        public MainForm()
        {
            this.Text = "AntiSleepDAC Konfigurace (v2.1.0)";
            this.Width = 450;
            this.Height = 280;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblInfo = new Label();
            lblInfo.Text = "Vyberte systémové zvukové zařízení pro posílání pulzu:";
            lblInfo.SetBounds(20, 20, 400, 20);

            cmbDevices = new ComboBox();
            cmbDevices.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevices.SetBounds(20, 45, 390, 25);
            
            var devices = SilentAudioPlayer.GetDevices();
            foreach (var d in devices) cmbDevices.Items.Add(d);
            
            cmbDevices.SelectedIndex = 0; // Default fallback

            Label lblInterval = new Label();
            lblInterval.Text = "Interval buzení (minuty):";
            lblInterval.SetBounds(20, 80, 130, 20);

            numInterval = new NumericUpDown();
            numInterval.Minimum = 1;
            numInterval.Maximum = 120;
            numInterval.Value = 25; // Default 25
            numInterval.SetBounds(160, 78, 60, 20);

            int savedInterval;
            string savedDevice = GetConfigDevice(out savedInterval);
            if(!string.IsNullOrEmpty(savedDevice))
            {
                int idx = devices.FindIndex(x => x.Trim() == savedDevice.Trim());
                if(idx >= 0) cmbDevices.SelectedIndex = idx;
            }
            if(savedInterval >= numInterval.Minimum && savedInterval <= numInterval.Maximum)
            {
                numInterval.Value = savedInterval;
            }

            btnSave = new Button();
            btnSave.Text = "Pouze uložit do config.ini";
            btnSave.SetBounds(20, 115, 160, 30);
            btnSave.Click += (s, e) => SaveConfig(cmbDevices.SelectedItem.ToString(), (int)numInterval.Value, false);

            btnSaveAndRun = new Button();
            btnSaveAndRun.Text = "Uložit a schovat na pozadí";
            btnSaveAndRun.SetBounds(190, 115, 220, 30);
            btnSaveAndRun.Click += (s, e) => SaveConfig(cmbDevices.SelectedItem.ToString(), (int)numInterval.Value, true);

            Label lblHint = new Label();
            lblHint.Text = "Po skrytí na pozadí najdete program v pravo dole v tray nabídce u hodin (ikonka 'i'). Klikněte pravým tlačítkem pro vypnutí nebo ukázání okna. Dlouhé názvy zařízení mohou být od Microsoftu zkráceny.";
            lblHint.SetBounds(20, 155, 390, 60);

            this.Controls.Add(lblInfo);
            this.Controls.Add(cmbDevices);
            this.Controls.Add(lblInterval);
            this.Controls.Add(numInterval);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnSaveAndRun);
            this.Controls.Add(lblHint);
        }

        string GetConfigDevice(out int interval)
        {
            interval = 25;
            if (File.Exists("config.ini"))
            {
                string[] lines = File.ReadAllLines("config.ini");
                if (lines.Length > 0)
                {
                    int parsed;
                    if (lines.Length > 1 && int.TryParse(lines[1], out parsed))
                        interval = parsed;
                    
                    return lines[0].Trim();
                }
            }
            return "";
        }

        void SaveConfig(string deviceName, int intervalMinutes, bool runHidden)
        {
            File.WriteAllLines("config.ini", new string[] { deviceName, intervalMinutes.ToString() });
            if (runHidden)
            {
                this.Hide();
                Thread t = new Thread(() => Program.RunLoop(deviceName, intervalMinutes));
                t.IsBackground = true;
                t.Start();
                
                NotifyIcon tray = new NotifyIcon();
                tray.Icon = SystemIcons.Information;
                tray.Text = "AntiSleepDAC v2.1.0 (" + deviceName + ")";
                tray.Visible = true;
                
                ContextMenu menu = new ContextMenu();
                menu.MenuItems.Add("Zobrazit okno s nastavením", (s, e) => { tray.Visible = false; this.Show(); });
                menu.MenuItems.Add("Ukončit a vypnout", (s, e) => { tray.Visible = false; Environment.Exit(0); });
                tray.ContextMenu = menu;
            }
            else
            {
                MessageBox.Show("Nastavení (zařízení a interval " + intervalMinutes + " min) bylo úspěšně uloženo do config.ini.", "Uloženo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0 && args[0] == "--hidden")
            {
                string deviceName = "";
                int interval = 25;
                if (File.Exists("config.ini"))
                {
                    string[] lines = File.ReadAllLines("config.ini");
                    if (lines.Length > 0) deviceName = lines[0].Trim();
                    int p;
                    if (lines.Length > 1 && int.TryParse(lines[1], out p)) interval = p;
                }
                
                RunLoop(deviceName, interval);
            }
            else
            {
                Application.Run(new MainForm());
            }
        }

        public static void RunLoop(string targetDevice, int intervalMinutes)
        {
            int deviceIndex = 0; // default mapping (-1 in WinMM)
            if (!string.IsNullOrEmpty(targetDevice))
            {
                var devices = SilentAudioPlayer.GetDevices();
                int idx = devices.FindIndex(x => x.Trim() == targetDevice.Trim());
                if (idx >= 0) deviceIndex = idx;
            }

            while (true)
            {
                try
                {
                    SilentAudioPlayer.PlaySilence(deviceIndex);
                }
                catch { }

                // Sleep dynamically assigned amount of minutes
                Thread.Sleep(intervalMinutes * 60 * 1000); 
            }
        }
    }
}
