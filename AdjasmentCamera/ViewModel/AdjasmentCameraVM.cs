using Accord;
using Accord.Video.FFMPEG;
using AdjasmentCamera.Helpers;
using EOSDigital.API;
using EOSDigital.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AdjasmentCamera.ViewModel
{
    public class AdjasmentCameraVM : NotifyPropertyChanged
    {

        #region value lable position 

        public System.Windows.Media.Brush BrushLblPosition { get; set; } = System.Windows.Media.Brushes.White;

        public Visibility LblPositionVisibility { get; set; } = Visibility.Hidden;

        private bool _iSLblMousePosition;

        public bool IsLblMousePosition
        {
            set
            {
                _iSLblMousePosition = value;
                if (_iSLblMousePosition == true) LblPositionVisibility = Visibility.Visible;
                if (_iSLblMousePosition == false) LblPositionVisibility = Visibility.Hidden;
            }
        }

        private bool _IsColorLblPositionBlack;

        public bool IsColorLblPositionBlack
        {
            set
            {
                _IsColorLblPositionBlack = value;
                if (_IsColorLblPositionBlack) IsColorLblPositionRed = false; IsColorLblPositionWhite = false; BrushLblPosition = System.Windows.Media.Brushes.Black;
            }
        }

        private bool _IsColorLblPositionWhite;

        public bool IsColorLblPositionWhite
        {
            set
            {
                _IsColorLblPositionWhite = value;
                if (_IsColorLblPositionWhite) IsColorLblPositionRed = false; IsColorLblPositionBlack = false; BrushLblPosition = System.Windows.Media.Brushes.White;
            }
        }

        private bool _IsColorLblPositionRed;

        public bool IsColorLblPositionRed
        {
            set
            {
                _IsColorLblPositionRed = value;
                if (_IsColorLblPositionRed) IsColorLblPositionWhite = false; IsColorLblPositionBlack = false; BrushLblPosition = System.Windows.Media.Brushes.Red;
            }
        }

        public string LblMousePosition { get; set; }

        #endregion

        #region valueBorder resizer

        private double _BorderResizerWidth = 100;

        public double BorderResizerWidth
        {
            get { return _BorderResizerWidth; }
            set
            {
                _BorderResizerWidth = value;
                ShowPositionElement(_moveBorder);
            }
        }

        private double _BorderResizerHeight = 100;

        public double BorderResizerHeight
        {
            get { return _BorderResizerHeight; }
            set
            {
                _BorderResizerHeight = value;
                ShowPositionElement(_moveBorder);
            }
        }

        private BorderResizerState _borderResizerState = BorderResizerState.Disable;

        public Visibility BorderResizerVisibility { get; set; } = Visibility.Hidden;

        public Brush BorderResizerEdgeBrush { get; set; } = System.Windows.Media.Brushes.Teal;

        public Brush BorderResizerBackgroundBrush { get; set; } = System.Windows.Media.Brushes.Transparent;

        private bool _IsColorBorderResizerBlack;

        public bool IsColorBorderResizerBlack
        {
            get { return _IsColorBorderResizerBlack; }
            set
            {
                _IsColorBorderResizerBlack = value;
                SetBrushBorderResizer();
            }
        }

        private bool _IsColorBorderResizerRed;

        public bool IsColorBorderResizerRed
        {
            get { return _IsColorBorderResizerRed; }
            set
            {
                _IsColorBorderResizerRed = value;
                SetBrushBorderResizer();
            }
        }

        private bool _IsColorBorderResizerTeal = true;

        public bool IsColorBorderResizerTeal
        {
            get { return _IsColorBorderResizerTeal; }
            set
            {
                _IsColorBorderResizerTeal = value;
                SetBrushBorderResizer();
            }
        }

        private bool _IsBackgroundBorderResizer;

        public bool IsBackgroundBorderResizer
        {
            get { return _IsBackgroundBorderResizer; }
            set
            {
                _IsBackgroundBorderResizer = value;
                SetBrushBorderResizer();
            }
        }

        public bool IsBorderResizer
        {
            set
            {
                if (_borderResizerState == BorderResizerState.Enable)
                {
                    _borderResizerState = BorderResizerState.Disable; BorderResizerVisibility = Visibility.Hidden;
                }
                else { _borderResizerState = BorderResizerState.Enable; BorderResizerVisibility = Visibility.Visible; }
            }
        }

        #endregion

        #region Camera value

        private Camera _mainCamera;
        private CanonAPI _cameraHandel;
        public CameraValue[] CameraListAv { get; set; }
        public CameraValue[] CameraListTv { get; set; }
        public CameraValue[] CameraListISO { get; set; }
        public List<Camera> CBAvailableCamera { get; set; }
        public CameraValue AvInit { get; set; }
        public CameraValue TvInit { get; set; }
        public CameraValue IsoInit { get; set; }
        #endregion

        #region Ui And Backend Value

        public string DistanceToWindowEdge { get; set; }

        public System.Windows.Point MoveResizer { get; set; }

        private Window _window;

        private System.Windows.Point _windowSize;

        public bool IsAnimationRunning { get; set; }

        private ProgressBar _progressBar;

        private ImageBrush _bgbrush = new ImageBrush();

        private Zoom _zoomNow = Zoom.NoZoom;

        private RecordByPc _recordByPcNow;

        private LiveState _liveStateThatNow { get; set; } = LiveState.StopLive;

        private RecordState _recordStateNow { get; set; } = RecordState.Stopped;

        public string LblBtnAlternateLive { get; set; } = "Click for start";

        public bool UiLock { get; set; } = false;

        public string LblBtnVideo { get; set; } = "Start Record";

        public ImageBrush LiveView { get; set; }

        public int BtnTakePhotoProgress { get; set; }

        public System.Windows.Point MoveToolsBorder { get; set; }

        public string LblError { get; set; }

        private BitmapImage _frame = new BitmapImage();

        private VideoFileWriter _writer;

        private DispatcherTimer _dispatcherTimer;

        private string _saveLocation;

        private Action<BitmapImage> _setImageAction;

        #endregion 

        #region Button Command and Button Value

        public RelayCommand BtnAlternateLive { get; set; }
        public RelayCommand BtnTakePhoto { get; set; }
        public RelayCommand BtnFocusNearMid { get; set; }
        public RelayCommand BtnFocusFarMid { get; set; }
        public RelayCommand BtnAutoFocus { get; set; }
        public RelayCommand BtnRecordVideo { get; set; }
        public RelayCommand BtnRefreshCameraList { get; set; }
        public RelayCommand BtnZoom { get; set; }
        public bool IsBtnMidQuality { get; set; }
        public bool IsBtnHighQuality { get; set; }
        #endregion

        public AdjasmentCameraVM()
        {
            #region Record By Pc

            _writer = new VideoFileWriter();
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(DispatcherTimer);
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(25);

            #endregion

            #region Provide Button

            BtnTakePhoto = new RelayCommand(TakePhoto);
            BtnAlternateLive = new RelayCommand(AlternateLive);
            BtnFocusNearMid = new RelayCommand(FocusNearMid);
            BtnFocusFarMid = new RelayCommand(FocusFarMid);
            BtnAutoFocus = new RelayCommand(AutoFocus);
            BtnRecordVideo = new RelayCommand(AlternateVideo);
            BtnZoom = new RelayCommand(ZoomCamera);

            #endregion

            #region Provide Start Camera

            try
            {
                _cameraHandel = new CanonAPI();
                CBAvailableCamera = _cameraHandel.GetCameraList();
                _setImageAction = (BitmapImage img) => { _frame = img; _bgbrush.ImageSource = img; };
                BtnRefreshCameraList = new RelayCommand(RefreshCameraList);
                _saveLocation = Directory.GetCurrentDirectory();
            }
            catch (DllNotFoundException)
            {
                File.Copy("EDSDK.dll", "C:\\Windows\\System\\EDSDK.dll");
                File.Copy("EDSDK.dll", "C:\\Windows\\System32\\EDSDK.dll");
                MessageBox.Show("dll added");
            }
            catch (Exception ex) { ReportError(ex.Message); }

            #endregion

        }

        private void ShowPositionElement(UIElement element)
        {
            System.Windows.Point borderLocation = element.TransformToAncestor(_window).Transform(new System.Windows.Point(0, 0));
            DistanceToWindowEdge = $"Top :{borderLocation.Y} \n Left :{borderLocation.X} \n Right:{_windowSize.X - (borderLocation.X + BorderResizerWidth) - 15} \n Bottom:{_windowSize.Y - (borderLocation.Y + BorderResizerHeight + 38)}";
        }

        #region Resize border
        private Border _border;

        public void ResizerBorder_Loaded(object sender, RoutedEventArgs e)
        {
            _border = sender as Border;
        }

        public void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var str = (string)((Thumb)sender).Name.ToLower();

            if (str.Contains("top"))
            {
                _border.Height = Math.Min(Math.Max(_border.MinHeight, _border.ActualHeight - e.VerticalChange), _border.MaxHeight);
                Canvas.SetTop(_border, Canvas.GetTop(_border) - _border.Height + _border.ActualHeight);
            }
            if (str.Contains("left"))
            {
                _border.Width = Math.Min(Math.Max(_border.MinWidth, _border.ActualWidth - e.HorizontalChange), _border.MaxWidth);
                Canvas.SetLeft(_border, Canvas.GetLeft(_border) - _border.Width + _border.ActualWidth);
            }
            if (str.Contains("bottom"))
            {
                _border.Height = Math.Min(Math.Max(_border.MinHeight, _border.ActualHeight + e.VerticalChange), _border.MaxHeight);
            }
            if (str.Contains("right"))
            {
                _border.Width = Math.Min(Math.Max(_border.MinWidth, _border.ActualWidth + e.HorizontalChange), _border.MaxWidth);
            }
            e.Handled = true;
        }

        #endregion

        #region Move element 

        private Border _moveBorder;

        private System.Windows.Point _moveStart;

        private Vector _moveSet;

        private string _nameContent;

        public void MouseDown(object sender, MouseButtonEventArgs e)
        {
            _moveBorder = sender as Border;

            _nameContent = (string)((Border)sender).Name.ToLower();

            _moveStart = e.GetPosition(_window);

            if (_nameContent.Contains("tools")) _moveSet = new Vector(MoveToolsBorder.X, MoveToolsBorder.Y);

            if (_nameContent.Contains("resizer")) _moveSet = new Vector(MoveResizer.X, MoveResizer.Y);

            _moveBorder.CaptureMouse();
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (_moveBorder.IsMouseCaptured)
                {
                    Vector offset = System.Windows.Point.Subtract(e.GetPosition(_window), _moveStart);

                    if (_nameContent.Contains("tools")) MoveToolsBorder = new System.Windows.Point(_moveSet.X + offset.X, _moveSet.Y + offset.Y);

                    if (_nameContent.Contains("resizer")) MoveResizer = new System.Windows.Point(_moveSet.X + offset.X, _moveSet.Y + offset.Y); ShowPositionElement(_moveBorder);
                }
            }
        }

        public void MouseUp(object sender, MouseButtonEventArgs e)
        {
            _moveBorder.ReleaseMouseCapture();
        }

        #endregion

        #region Focus Camera

        private void FocusNearMid(object obj)
        {
            try { _mainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Near2); }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void FocusFarMid(object obj)
        {
            try { _mainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Far2); }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void AutoFocus(object obj)
        {
            try
            {
                Task.Run(async () =>
                {
                    _mainCamera.SendCommand(CameraCommand.DoEvfAf, 1);
                    await Task.Delay(4000);
                    _mainCamera.SendCommand(CameraCommand.DoEvfAf, 0);
                });
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        #endregion

        #region Record Video

        private void RecordVideoByPc()
        {
            if (_recordStateNow == RecordState.Start)
            {
                LblBtnVideo = "Start Record";
                _dispatcherTimer.Stop();
                StopRecordingByPc();
                _recordStateNow = RecordState.Stopped;
                _recordByPcNow = RecordByPc.Stopped;
            }
            else
            {
                _writer.Open("Video.mp4", 1056, 704, 25, VideoCodec.MPEG4);
                _dispatcherTimer.Start();
                _recordByPcNow = RecordByPc.Start;
                _recordStateNow = RecordState.Start;
                LblBtnVideo = "Stop record";
            }
        }

        private void AlternateVideo(object obj)
        {
            if (IsBtnHighQuality) RecordVideoByCamera();
            if (IsBtnMidQuality) RecordVideoByPc();

        }

        private void RecordVideoByCamera()
        {
            try
            {
                if (_recordStateNow == RecordState.Start)
                {
                    LblBtnVideo = "Start Record";
                    _mainCamera.StopFilming(true);
                    _recordStateNow = RecordState.Stopped;
                }
                else
                {
                    _mainCamera.SetCapacity(4096, int.MaxValue);
                    _mainCamera.StartFilming(true);
                    _recordStateNow = RecordState.Start;
                    LblBtnVideo = "Stop record";
                    LblBtnAlternateLive = LiveState.StartLive.GetDescription();
                    _liveStateThatNow = LiveState.StopLive;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void DispatcherTimer(object sender, EventArgs e)
        {
            _writer.WriteVideoFrame(BitmapImageToBitmap.Execute(_frame));
        }

        private void StopRecordingByPc()
        {
            _writer.Close();
        }

        public void IsBtnMidQuality_Checked(object sender, RoutedEventArgs e)
        {
            IsBtnHighQuality = false;
        }

        public void IsBtnHighQuality_Checked(object sender, RoutedEventArgs e)
        {
            IsBtnMidQuality = false;
        }

        #endregion

        #region Live

        private void AlternateLive(object obj)
        {
            try
            {
                if (_liveStateThatNow == LiveState.StartLive)
                {
                    LblBtnAlternateLive = LiveState.StartLive.GetDescription();
                    _liveStateThatNow = LiveState.StopLive;
                    _mainCamera.StopLiveView();
                }

                else
                {
                    LblBtnAlternateLive = LiveState.StopLive.GetDescription();
                    _liveStateThatNow = LiveState.StartLive;
                    _mainCamera.StartLiveView();
                    LiveView = _bgbrush;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void MainCamera_LiveViewUpdated(Camera sender, Stream img)
        {
            try
            {
                using (WrapStream s = new WrapStream(img))
                {
                    img.Position = 0;
                    BitmapImage EvfImage = new BitmapImage();
                    EvfImage.BeginInit();
                    EvfImage.StreamSource = s;
                    EvfImage.CacheOption = BitmapCacheOption.OnLoad;
                    EvfImage.EndInit();
                    EvfImage.Freeze();
                    Application.Current.Dispatcher.BeginInvoke(_setImageAction, EvfImage);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        #endregion

        #region Camera Settings

        public void CameraISO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ISO = (((ComboBox)sender).SelectedItem).ToString();
            try { _mainCamera.SetSetting(PropertyID.ISO, ISOValues.GetValue(ISO).IntValue); }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        public void CameraTv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string Tv = (((ComboBox)sender).SelectedItem).ToString();
                _mainCamera.SetSetting(PropertyID.Tv, TvValues.GetValue(Tv).IntValue);
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        public void CameraAv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Av = (((ComboBox)sender).SelectedItem).ToString();
            try { _mainCamera.SetSetting(PropertyID.Av, AvValues.GetValue(Av).IntValue); }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void provideStart()
        {
            try
            {
                _mainCamera.OpenSession();
                _mainCamera.SetSetting(EOSDigital.SDK.PropertyID.SaveTo, (int)SaveTo.Host);
                _mainCamera.SetCapacity(4096, int.MaxValue);
                _mainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
                _mainCamera.ProgressChanged += MainCamera_ProgressChanged;
                _mainCamera.DownloadReady += MainCamera_DownloadReady;
                RefreshValueList();
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void RefreshValueList()
        {
            CameraListAv = _mainCamera.GetSettingsList(PropertyID.Av);
            CameraListTv = _mainCamera.GetSettingsList(PropertyID.Tv);
            CameraListISO = _mainCamera.GetSettingsList(PropertyID.ISO);
            AvInit = AvValues.GetValue(_mainCamera.GetInt32Setting(PropertyID.Av));
            TvInit = TvValues.GetValue(_mainCamera.GetInt32Setting(PropertyID.Tv));
            IsoInit = ISOValues.GetValue(_mainCamera.GetInt32Setting(PropertyID.ISO));
        }

        #endregion 

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _window = sender as Window;
            _windowSize = new System.Windows.Point(_window.ActualWidth, _window.ActualHeight);
        }

        public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _window = sender as Window;
            _windowSize = new System.Windows.Point(_window.ActualWidth, _window.ActualHeight);
        }

        private void SetBrushBorderResizer()
        {
            if (IsColorBorderResizerBlack) BorderResizerEdgeBrush = System.Windows.Media.Brushes.Black;

            if (IsColorBorderResizerTeal) BorderResizerEdgeBrush = System.Windows.Media.Brushes.Teal;

            if (IsColorBorderResizerRed) BorderResizerEdgeBrush = System.Windows.Media.Brushes.Red;

            if (IsBackgroundBorderResizer) { BorderResizerBackgroundBrush = BorderResizerEdgeBrush; }

            if (!IsBackgroundBorderResizer) { BorderResizerBackgroundBrush = System.Windows.Media.Brushes.Transparent; };
        }

        private void TakePhoto(object obj)
        {
            try
            {
                _mainCamera.TakePhoto();
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void ZoomCamera(object obj)
        {
            try
            {
                switch (_zoomNow)
                {
                    case Zoom.NoZoom: _zoomNow = Zoom.X5; _mainCamera.SetSetting(PropertyID.Evf_Zoom, 5); break;
                    case Zoom.X5: _zoomNow = Zoom.NoZoom; _mainCamera.SetSetting(PropertyID.Evf_Zoom, 1); break;
                }
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        public void AvailableCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _mainCamera = ((ComboBox)sender).SelectedItem as Camera;
            UiLock = true;
            provideStart();
        }

        private void MainCamera_ProgressChanged(object sender, int progress)
        {
            try { _progressBar.Dispatcher.Invoke((Action)delegate { _progressBar.Value = progress; BtnTakePhotoProgress = progress; }); }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void MainCamera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            try
            {
                sender.DownloadFile(Info, _saveLocation);
                _progressBar.Dispatcher.Invoke((Action)delegate { _progressBar.Value = 0; });
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        public void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            _progressBar = sender as ProgressBar;
        }

        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _mainCamera?.Dispose();
                _cameraHandel?.Dispose();
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }

        private void RefreshCameraList(object obj)
        {
            CBAvailableCamera.Clear();
            CBAvailableCamera = _cameraHandel.GetCameraList();
        }

        public void MouseMovePosition(object sender, MouseEventArgs e)
        {
            Canvas canvas = sender as Canvas;
            LblMousePosition = "X :" + (e.GetPosition(canvas).X).ToString() + "\n" + "Y :" + (e.GetPosition(canvas).Y).ToString();
        }

        private void ReportError(string message)
        {
            LblError = message;
            IsAnimationRunning = true;
            IsAnimationRunning = false;
        }

        public void LblError_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LblError = null;
        }

        #region enum value

        public enum LiveState
        {
            [Description("Start Live")]
            StartLive,
            [Description("Stop Live")]
            StopLive,
        }

        public enum RecordState
        {
            [Description("Stopped record")]
            Stopped,
            [Description("In record")]
            Start,
        }

        public enum RecordByPc
        {
            Stopped,
            Start,
        }

        public enum Zoom
        {
            X10,
            X5,
            NoZoom,
        }

        public enum BorderResizerState
        {
            Enable,
            Disable,
        }

        #endregion 
    }
}
