using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VRC_Discord_1
{

    public class VRCConfig
    {
        public bool disableRichPresence { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _statusText;
        private string _currentStatus;
        private Guid localLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public string CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                _currentStatus = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Check VRC config status on startup
            CheckVRCConfigStatus();
        }

        private void EnableClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Enable clicked");
            StatusText = "VRC Discord Presence が有効化されました";
            CurrentStatus = "有効";
            WriteVRCConfigValue(false);
        }

        private void DisableClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Disable clicked");
            StatusText = "VRC Discord Presence が無効化されました";
            CurrentStatus = "無効";
            WriteVRCConfigValue(true);
        }

        private void CheckVRCConfigStatus()
        {
            try
            {
                string appDataPath = this.GetFolderLocalLow();
                string vrcCfgPath = System.IO.Path.Combine(appDataPath, "VRChat\\VRChat\\config.json");

                if (File.Exists(vrcCfgPath))
                {
                    string json = File.ReadAllText(vrcCfgPath);
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    
                    if (jsonObj != null && jsonObj["disableRichPresence"] != null)
                    {
                        bool isDisabled = jsonObj["disableRichPresence"];
                        if (isDisabled)
                        {
                            CurrentStatus = "無効";
                            StatusText = "VRC Discord Presence は現在無効です";
                        }
                        else
                        {
                            CurrentStatus = "有効";
                            StatusText = "VRC Discord Presence は現在有効です";
                        }
                    }
                    else
                    {
                        // Config exists but no disableRichPresence property
                        CurrentStatus = "有効";
                        StatusText = "VRC Discord Presence は現在有効です";
                    }
                }
                else
                {
                    // No config file exists, default to enabled
                    CurrentStatus = "有効";
                    StatusText = "VRC Discord Presence は現在有効です";
                }
            }
            catch (Exception ex)
            {
                // Error reading config, default to unknown status
                CurrentStatus = "不明";
                StatusText = "設定ファイルの読み込みに失敗しました";
                Trace.WriteLine($"Error reading VRC config: {ex.Message}");
            }
        }

        private void WriteVRCConfigValue(bool disableRichPresence)
        {
            string appDataPath = this.GetFolderLocalLow();
            string vrcCfgPath = System.IO.Path.Combine(appDataPath, "VRChat\\VRChat\\config.json");

            // Check if config.json exists
            if (File.Exists(vrcCfgPath))
            {
                // File exists, read and update it
                string json = File.ReadAllText(vrcCfgPath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                jsonObj["disableRichPresence"] = disableRichPresence;
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(vrcCfgPath, output);
                //TextBlockValue = "Config file updated successfully!";
            }
            else
            {
                // File doesn't exist, create it with default values
                VRCConfig vRC = new VRCConfig
                {
                    disableRichPresence = disableRichPresence
                };

                string jsonOut = JsonSerializer.Serialize(vRC, new JsonSerializerOptions { WriteIndented = true });

                // Ensure the directory exists
                _ = Directory.CreateDirectory(System.IO.Path.GetDirectoryName(vrcCfgPath));

                File.WriteAllText(vrcCfgPath, jsonOut);
                //TextBlockValue = "Config file created with new values!";
            }
        }

        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath(
           [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
           uint dwFlags,
           IntPtr hToken,
           out IntPtr pszPath);

        public string GetFolderLocalLow()
        {
            var pszPath = IntPtr.Zero;
            try
            {
                var hr = SHGetKnownFolderPath(this.localLowId, 0, IntPtr.Zero, out pszPath);
                if (hr < 0)
                {
                    throw Marshal.GetExceptionForHR(hr);
                }
                return Marshal.PtrToStringAuto(pszPath);
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pszPath);
                }
            }

        }
    }
}