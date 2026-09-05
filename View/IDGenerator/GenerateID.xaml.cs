using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SPTC_APPLICATION.Objects;
// NOTE: AForge.Video and AForge.Video.DirectShow are .NET Framework-only libraries
// and are NOT compatible with .NET 8. The camera capture functionality using AForge
// must be replaced with a .NET 8-compatible alternative such as:
//   - DirectShowLib-2005 (via NuGet: DirectShowLib)
//   - OpenCvSharp4 (via NuGet: OpenCvSharp4.Windows)
//   - Windows.Media.Capture (WinRT API, Windows 10+)
// The AForge-dependent code below is wrapped in #if AFORGE_AVAILABLE to prevent
// compilation errors. Replace with the chosen alternative library.
// TODO: Replace AForge camera capture with a .NET 8-compatible library.

namespace SPTC_APPLICATION.View
{
    /// <summary>
    /// Interaction logic for GenerateID.xaml
    /// </summary>
    public partial class GenerateID : Window
    {
        BitmapSource? lastCapturedImage = null;
        bool hasPhoto = false;
        bool hasSign = false;
        bool isDriver = true;
        private Franchise? franchise;
        bool isUpdate = false;

        // AForge camera objects - commented out pending replacement with .NET 8-compatible library
        // private FilterInfoCollection videoDevices;
        // private VideoCaptureDevice videoSource;

        public GenerateID()
        {
            InitializeComponent();
            tboxAddressS.Text = AppState.DEFAULT_ADDRESSLINE2;
            bDay.SelectedDate = DateTime.Today;
            EventLogger.Post("VIEW :: ID GENERATE Window");
            // TODO: Replace AForge camera initialization with .NET 8-compatible library
            // videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            btnStartPad.IsEnabled = false;
            btnStartCam.IsEnabled = false; // Disabled until AForge replacement is implemented
            EventLogger.Post("WARN :: Camera capture requires AForge replacement for .NET 8. See GenerateID.xaml.cs TODO comments.");
            franchise = new Franchise();
            isUpdate = false;
        }

        public GenerateID(Franchise franchise, bool isDriver)
        {
            InitializeComponent();
            bDay.SelectedDate = DateTime.Today;
            this.franchise = franchise;
            isUpdate = true;
            EventLogger.Post("VIEW :: ID GENERATE Window id=" + franchise.id);
            // TODO: Replace AForge camera initialization with .NET 8-compatible library
            btnStartPad.IsEnabled = false;
            btnStartCam.IsEnabled = false; // Disabled until AForge replacement is implemented

            this.isDriver = isDriver;
            MySwitch.Visibility = Visibility.Hidden;
            if (isDriver)
            {
                drvOrOprt.Content = "DRIVER";
                lblPhoto.Content = "Driver's Photo";
                lblsign.Content = "Driver's Signature";
                if (franchise.Driver_day != null)
                {
                    var drv = franchise.Driver_day;
                    if (drv.name != null)
                    {
                        cbGender.SelectedIndex = ((drv.name.prefix == "Mrs.") ? 1 : 0);
                        tboxFn.Text = drv.name.firstname;
                        tboxMn.Text = drv.name.middlename;
                        tboxLn.Text = drv.name.lastname;
                    }
                    if (drv.address != null)
                    {
                        tboxAddressB.Text = drv.address.addressline1;
                        tboxAddressS.Text = drv.address.addressline2;
                    }
                    if (drv.image != null)
                    {
                        imgIDPic.Source = drv.image.GetSource();
                        hasPhoto = true;
                    }
                    if (drv.signature != null)
                    {
                        imgSignPic.Source = drv.signature.GetSource();
                        hasSign = true;
                    }
                    if (drv.birthday != null)
                    {
                        bDay.DisplayDate = drv.birthday;
                        bDay.DataContext = drv.birthday;
                        bDay.Text = drv.birthday.ToString();
                    }
                    tboxEmePer.Text = drv.emergencyPerson;
                    tboxPhone.Text = drv.emergencyContact;
                    tboxBnum.Text = franchise.bodynumber;
                    tboxLnum.Text = franchise.licenceNO;
                }
            }
            else
            {
                drvOrOprt.Content = "OPERATOR";
                lblPhoto.Content = "Operator's Photo";
                lblsign.Content = "Operator's Signature";
                if (franchise.Operator != null)
                {
                    var drv = franchise.Operator;
                    if (drv.name != null)
                    {
                        cbGender.SelectedIndex = ((drv.name.prefix == "Mrs.") ? 1 : 0);
                        tboxFn.Text = drv.name.firstname;
                        tboxMn.Text = drv.name.middlename;
                        tboxLn.Text = drv.name.lastname;
                    }
                    if (drv.address != null)
                    {
                        tboxAddressB.Text = drv.address.addressline1;
                        tboxAddressS.Text = drv.address.addressline2;
                    }
                    if (drv.image != null)
                    {
                        imgIDPic.Source = drv.image.GetSource();
                        hasPhoto = true;
                    }
                    if (drv.signature != null)
                    {
                        imgSignPic.Source = drv.signature.GetSource();
                        hasSign = true;
                    }
                    if (drv.birthday != null)
                    {
                        bDay.DisplayDate = drv.birthday;
                        bDay.DataContext = drv.birthday;
                        bDay.Text = drv.birthday.ToString();
                    }
                    tboxEmePer.Text = drv.emergencyPerson;
                    tboxPhone.Text = drv.emergencyContact;
                    tboxBnum.Text = franchise.bodynumber;
                    tboxLnum.Text = franchise.licenceNO;
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.StopCamera();
            base.OnClosing(e);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.StopCamera();
            PrintPreview print = new PrintPreview();
            print.Show();
            this.Close();
        }

        private void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            pbCameraOpen.Value = pbCameraOpen.Minimum;
        }

        private void MySwitch_Back(object sender, RoutedEventArgs e)
        {
            isDriver = false;
            drvOrOprt.Content = "Create this ID for\nOperator.";
        }

        private void MySwitch_Front(object sender, RoutedEventArgs e)
        {
            isDriver = true;
            drvOrOprt.Content = "Create this ID for\nDriver.";
        }

        // TODO: Replace this method with .NET 8-compatible camera frame handler
        // Original used AForge.Video.NewFrameEventArgs - not available in .NET 8
        // private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs) { ... }

        private async void BtnStartCam_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement camera capture using a .NET 8-compatible library
            // AForge.Video.DirectShow is not supported on .NET 8
            ControlWindow.ShowDialog("Camera Not Available",
                "Camera capture requires AForge replacement.\nPlease use 'Browse' to select an image file.",
                Icons.NOTIFY);
            await Task.CompletedTask;
        }

        private void btnBrowseIDPic_Click(object sender, RoutedEventArgs e)
        {
            this.StopCamera();
            var openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(selectedFilePath);
                bitmapImage.EndInit();

                imgIDPic.Source = bitmapImage;
                hasPhoto = true;
            }
        }

        private void btnBrowseSignPic_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(selectedFilePath);
                bitmapImage.EndInit();

                imgSignPic.Source = bitmapImage;
                hasSign = true;
            }
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (tboxLnum.Text.Length > 0 && tboxFn.Text.Length > 0 && tboxLn.Text.Length > 0 && tboxAddressB.Text.Length > 0 && tboxAddressS.Text.Length > 0 && tboxEmePer.Text.Length > 0 && tboxPhone.Text.Length > 0)
            {
                GeneratedIDPreview print = new GeneratedIDPreview();
                print.ReturnControl(this);
                if (isDriver)
                {
                    Driver @obj = new Driver();
                    SPTC_APPLICATION.Objects.Image? image = null;
                    SPTC_APPLICATION.Objects.Image? sign = null;
                    if (hasPhoto)
                    {
                        image = new SPTC_APPLICATION.Objects.Image(imgIDPic.Source, $"Drv - {tboxFn.Text}");
                    }
                    if (hasSign)
                    {
                        sign = new SPTC_APPLICATION.Objects.Image(imgSignPic.Source, $"Sign  -{tboxFn.Text}");
                    }

                    string prefix = (cbGender.SelectedIndex == 0) ? "Mr." : "Mrs.";
                    if (isUpdate && franchise != null)
                    {
                        @obj = franchise.Driver_day!;
                        Name name = @obj.name!;
                        name.prefix = prefix;
                        name.firstname = tboxFn.Text;
                        name.middlename = tboxMn.Text;
                        name.lastname = tboxLn.Text;
                        Address address = @obj.address!;
                        address.addressline1 = tboxAddressB.Text;
                        address.addressline2 = tboxAddressS.Text;
                        @obj.WriteInto(name, address, image, sign, "", (DateTime)bDay.SelectedDate!, tboxEmePer.Text, tboxPhone.Text);
                    }
                    else
                    {
                        Name name = new Name(prefix, tboxFn.Text, tboxMn.Text, tboxLn.Text, "");
                        Address address = new Address(tboxAddressB.Text, tboxAddressS.Text);
                        @obj.WriteInto(name, address, image, sign, "", (DateTime)bDay.SelectedDate!, tboxEmePer.Text, tboxPhone.Text);
                    }

                    franchise!.WriteInto(tboxBnum.Text, null, @obj, null, tboxLnum.Text);

                    ID id = new ID(franchise, Objects.General.DRIVER_DAY);
                    print.Save(id);
                    print.Show();
                    this.Hide();
                }
                else
                {
                    Operator @obj = new Operator();
                    SPTC_APPLICATION.Objects.Image? image = null;
                    SPTC_APPLICATION.Objects.Image? sign = null;
                    if (hasPhoto)
                    {
                        image = new SPTC_APPLICATION.Objects.Image(imgIDPic.Source, $"Drv - {tboxFn.Text}");
                    }
                    if (hasSign)
                    {
                        sign = new SPTC_APPLICATION.Objects.Image(imgSignPic.Source, $"Sign  -{tboxFn.Text}");
                    }
                    string prefix = (cbGender.SelectedIndex == 0) ? "Mr." : "Mrs.";
                    if (isUpdate && franchise != null)
                    {
                        @obj = franchise.Operator!;
                        Name name = @obj.name!;
                        name.prefix = prefix;
                        name.firstname = tboxFn.Text;
                        name.middlename = tboxMn.Text;
                        name.lastname = tboxLn.Text;
                        Address address = @obj.address!;
                        address.addressline1 = tboxAddressB.Text;
                        address.addressline2 = tboxAddressS.Text;
                        @obj.WriteInto(name, address, image, sign, "", (DateTime)bDay.SelectedDate!, tboxEmePer.Text, tboxPhone.Text);
                    }
                    else
                    {
                        Name name = new Name(prefix, tboxFn.Text, tboxMn.Text, tboxLn.Text, "");
                        Address address = new Address(tboxAddressB.Text, tboxAddressS.Text);
                        @obj.WriteInto(name, address, image, sign, "", (DateTime)bDay.SelectedDate!, tboxEmePer.Text, tboxPhone.Text);
                    }

                    franchise!.WriteInto(tboxBnum.Text, @obj, null, null, tboxLnum.Text);

                    ID id = new ID(franchise, Objects.General.OPERATOR);
                    print.Save(id);
                    print.Show();
                    this.Hide();
                }
            }
            else
            {
                ControlWindow.ShowDialog("Input Fields incomplete!", "Missing some required inputs.");
            }
        }

        public void StopCamera()
        {
            // TODO: Stop camera using .NET 8-compatible library when AForge is replaced
            // No-op for now since AForge is not available in .NET 8
        }
    }
}
