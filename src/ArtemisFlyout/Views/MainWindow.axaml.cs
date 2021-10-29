#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using ArtemisFlyout.Util;
using ArtemisFlyout.ViewModels;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia.Enums;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;
using Window = Avalonia.Controls.Window;
 
namespace ArtemisFlyout.Views
{
    public partial class MainWindow : Window
    {
        private const int FlyoutHorizontalSpacing = 12;
        private const int FlyoutVerticalSpacing = 25;
        private const int AnimationDelay = 200;

        private int _flyoutWidth;
        private int _flyoutHeight;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            var primaryScreen = Screens.Primary.WorkingArea;

            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.Manual;

            _flyoutWidth = (int)this.Find<Panel>("FlyoutPanelContainer").Width + FlyoutHorizontalSpacing;
            _flyoutHeight = (int)this.Find<Panel>("FlyoutPanelContainer").Height + FlyoutVerticalSpacing;

            Position = new PixelPoint(primaryScreen.Width - _flyoutWidth, primaryScreen.Height - _flyoutHeight);
            Deactivated += MainWindow_Deactivated;
        }

        private bool _animating;
        public async void ShowAnimated()
        {
            if (_animating)
                return;
            _animating = true;

            Show();
            var filler = this.Find<Separator>("SepAnimationFiller");
            Width = _flyoutWidth;
            Height = _flyoutHeight;

            var t = new DoubleTransition()
            {

                Property = Separator.WidthProperty,
                Duration = TimeSpan.FromMilliseconds(AnimationDelay),
                Easing = new CircularEaseOut()
            };

            t.Apply(filler, Avalonia.Animation.Clock.GlobalClock, (double)_flyoutWidth, 0d);

            _animating = false;
        }

        public async void CloseAnimated()
        {
            if (_animating)
                return;
            _animating = true;

            var filler = this.Find<Separator>("SepAnimationFiller");
            var t = new DoubleTransition()
            {

                Property = Separator.WidthProperty,
                Duration = TimeSpan.FromMilliseconds(AnimationDelay),
                Easing = new CircularEaseIn()
            };

            t.Apply(filler, Avalonia.Animation.Clock.GlobalClock, 0d, (double)_flyoutWidth);

            // -10 is enough to avoid windows flashing
            await Task.Delay(AnimationDelay - 10);
            Close();
            Program.MainWindowInstance = null;

            _animating = false;
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            CloseAnimated();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void BtnRestart_OnClick(object? sender, RoutedEventArgs e)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Artemis", "Are you sure you want restart Artemis?", ButtonEnum.YesNo, MessageBox.Avalonia.Enums.Icon.Warning);
            var result = await messageBoxStandardWindow.Show();

            if (result != ButtonResult.Yes)
                return;
            RestUtil.RestGetBool("http://127.0.0.1", 9696, "/remote/restart");
            CloseAnimated();
        }

        private async void BtnHome_OnClick(object? sender, RoutedEventArgs e)
        {
            RestUtil.RestGetBool("http://127.0.0.1", 9696, "/remote/bring-to-foreground");
        }

        private async void BtnWorkshop_OnClick(object? sender, RoutedEventArgs e)
        {
            RestUtil.RestGetBool("http://127.0.0.1", 9696, "/windows/show-workshop");
            CloseAnimated();
        }

        private async void BtnShowSurfaceEditor_OnClick(object? sender, RoutedEventArgs e)
        {
            RestUtil.RestGetBool("http://127.0.0.1", 9696, "/windows/show-surface-editor");
            CloseAnimated();
        }

        private async void BtnShowDebugger_OnClick(object? sender, RoutedEventArgs e)
        {
            RestUtil.RestGetBool("http://127.0.0.1", 9696, "/windows/show-debugger");
            CloseAnimated();
        }

        private async void BtnShowSettings_OnClick(object? sender, RoutedEventArgs e)
        {
            RestUtil.RestGetBool("http://127.0.0.1", 9696, "/windows/show-settings");
            CloseAnimated();
        }

        public static void CreateAndShow()
        {
            if (Program.MainWindowInstance == null)
            {
                Program.MainWindowInstance = new MainWindow();
                MainWindow flyout = Program.MainWindowInstance;
                flyout.DataContext = new MainWindowViewModel();
            }
            Program.MainWindowInstance.ShowAnimated();
        }

        public static async void Preload()
        {
            var prelodWindow = new MainWindow();
            prelodWindow.Opacity = 0;
            prelodWindow.ShowAnimated();
            Thread.Sleep(500);
            prelodWindow.Close();
            Thread.Sleep(500);
        }

        public void SetContentPageIndex(int index)
        {
            this.Find<Carousel>("CarouselContentContainer").SelectedIndex = index;
        }

    }
}
