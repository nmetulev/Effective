using EffectiveEffect;
using System;
using Windows.Media.Capture;
using Windows.Media.Effects;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Effective
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture _mediaCapture;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings()
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
            };
            await _mediaCapture.InitializeAsync(settings);
            captureElement.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();

            var effect = await _mediaCapture.AddVideoEffectAsync(
                new VideoEffectDefinition(typeof(TheEffect).FullName),
                                            MediaStreamType.VideoPreview);
        }
    }
}
