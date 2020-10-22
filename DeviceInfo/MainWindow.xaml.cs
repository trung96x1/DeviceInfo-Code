using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//Use to get device info
//Need to add ADB folder to same dir with exe file
namespace DeviceInfo {

    public class Device {
        public String serial {get;set;}
        public String name {get;set;}
        public String state {get;set;}
    }
    public class DeviceProp {
        public String property {get;set;}
        public String value {get;set;}
    }
    public class StateToColor : IValueConverter {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            if("device".Equals(value.ToString()))
                return "#FF52F307";
            return "#FFFB5656";
        }
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            return null;
        }
    }
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            this.Icon = BitmapFrame.Create(new Uri(Environment.CurrentDirectory + @"\Resource\iconDevice.ico"));
            showListDevice();
        }
        private void showListDevice() {
            List<Device> listDevice = new List<Device>();
            String result = ExecuteCMD("adb devices -l");
            String[] listResult = result.Split(new String[] { "\r\n" }, StringSplitOptions.None);
            foreach(String line in listResult) {
                if(line.Contains("model")) {
                    String[] info = System.Text.RegularExpressions.Regex.Split(line, @"\s{1,}");
                    Device device = new Device();
                    device.serial = info[0].Trim();
                    device.state = info[1].Trim();
                    device.name = info[3].Trim().Substring(9);
                    listDevice.Add(device);
                }
                if(line.Contains("unauthorized")) {
                    String[] info = System.Text.RegularExpressions.Regex.Split(line, @"\s{1,}");
                    Device device = new Device();
                    device.serial = info[0].Trim();
                    device.state = info[1].Trim();
                    device.name = "State";
                    listDevice.Add(device);
                }
            }
            lvDevice.ItemsSource = listDevice;
        }

        public static string ExecuteCMD( string cmdCommand ) {
            try {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo() {
                    WorkingDirectory = Environment.CurrentDirectory + @"\ADB",
                    FileName = "cmd.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };
                process.Start();
                process.StandardInput.WriteLine(cmdCommand);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                // process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
            } catch {
                return (string)null;
            }
        }
        private void listViewItem_MouseDoubleClick( object sender, MouseButtonEventArgs e ) {
            ListViewItem item = sender as ListViewItem;
            Device device = item.Content as Device;
            List<DeviceProp> listProp = new List<DeviceProp>();
            String result2 = ExecuteCMD("adb -s " + device.serial + " shell getprop");
            String[] listResult2 = result2.Split(new String[] { "\r\n" }, StringSplitOptions.None);
            DeviceProp salecode = new DeviceProp();
            salecode.property = "Sale code";
            DeviceProp numeric = new DeviceProp();
            numeric.property = "Numeric";
            DeviceProp cscver = new DeviceProp();
            cscver.property = "CSC version";
            DeviceProp buildid = new DeviceProp();
            buildid.property = "Build ID";
            DeviceProp syncl = new DeviceProp();
            syncl.property = "Syn CL";
            DeviceProp isrelease = new DeviceProp();
            isrelease.property = "Release ver";
            DeviceProp buildtype = new DeviceProp();
            buildtype.property = "Build type";
            DeviceProp chipset = new DeviceProp();
            chipset.property = "Chipset";
            foreach(String line in listResult2) {
                if(line.Contains("[ro.csc.sales_code]"))
                    salecode.value = getValue(line);
                if(line.Contains("[gsm.sim.operator.numeric]"))
                    numeric.value = getValue(line);
                if(line.Contains("[ril.official_cscver]"))
                    cscver.value = getValue(line);
                if(line.Contains("[ro.omc.build.id]"))
                    buildid.value = getValue(line);
                if(line.Contains("[ro.build.changelist]"))
                    syncl.value = getValue(line);
                if(line.Contains("[ro.build.official.release]"))
                    isrelease.value = getValue(line);
                if(line.Contains("[ro.build.type]"))
                    buildtype.value = getValue(line);
                if(line.Contains("[ro.hardware]"))
                    chipset.value = getValue(line);
            }
            listProp.Add(salecode);
            listProp.Add(numeric);
            listProp.Add(cscver);
            listProp.Add(buildid);
            listProp.Add(syncl);
            listProp.Add(isrelease);
            listProp.Add(buildtype);
            listProp.Add(chipset);
            lvDeviceInfo.ItemsSource = listProp;
        }
        public String getValue( String line ) {
            int start = line.IndexOf(':') + 3;
            int end = line.Length - 1;
            return line.Substring(start, end - start);
        }
        private void Button_Click_1( object sender, RoutedEventArgs e ) {
            showListDevice();
        }
    }

}
