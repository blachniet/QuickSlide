using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickSlide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Dependency Properties

        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        #endregion
        
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        #endregion

        #region Event Handlers

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == WindowStateProperty.Name)
            {
                var newValue = (WindowState)e.NewValue;
                if (newValue != WindowState.Maximized)
                    ToggleFullScreen();
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var imgFiles in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.jpg"))
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(imgFiles, UriKind.Absolute);
                src.EndInit();
                _imageSources.Add(src);
            }

            if (_imageSources.Count > 0)
            {
                _currentIndex = 0;
                ImageDisplay.Source = _imageSources[_currentIndex];

                _timer.Interval = 50;
                _timer.Elapsed += Timer_Elapsed;
                _timer.Start();
            }
        }

        void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F11:
                    ToggleFullScreen();
                    break;

                case Key.Space:
                case Key.Right:
                    NextImage();
                    _currentImageTime = 0;
                    break;

                case Key.Left:
                    PrevImage();
                    _currentImageTime = 0;
                    break;

                case Key.Up:
                    _imageChangeInterval += 5000;
                    break;

                case Key.Down:
                    if (_imageChangeInterval > 5000)
                        _imageChangeInterval -= 5000;
                    break;

                case Key.Escape:
                    if (IsFullScreen)
                        ToggleFullScreen();
                    else
                        Close();
                    break;
            }
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _currentImageTime += _timer.Interval;
            if (_currentImageTime >= _imageChangeInterval)
            {
                this.Dispatcher.InvokeAsync(() => NextImage());
                _currentImageTime = 0;
            }
            
            this.Dispatcher.InvokeAsync(() => UpdateProgress());
        }

        #endregion

        #region Internal Methods

        private void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
            WindowState = IsFullScreen ? WindowState.Maximized : WindowState.Normal;
            WindowStyle = IsFullScreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;
        }

        private void NextImage()
        {
            if (_currentIndex >= 0)
            {
                _currentIndex = (++_currentIndex) % _imageSources.Count;
                ImageDisplay.Source = _imageSources[_currentIndex];
            }
        }

        private void PrevImage()
        {
            if (_currentIndex >= 0)
            {
                if (_currentIndex == 0)
                    _currentIndex = _imageSources.Count - 1;
                else
                    --_currentIndex;

                ImageDisplay.Source = _imageSources[_currentIndex];
            }
        }

        private void UpdateProgress()
        {
            ProgressBar.Value = _currentImageTime / _imageChangeInterval;
        }

        #endregion

        #region Fields

        private double _imageChangeInterval = 30000;
        private double _currentImageTime;
        private int _currentIndex = -1;
        private List<BitmapImage> _imageSources = new List<BitmapImage>();
        private Timer _timer = new Timer();

        #endregion
    }
}
