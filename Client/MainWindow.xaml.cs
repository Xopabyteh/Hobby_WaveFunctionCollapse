using System.Drawing;
using System.Windows;
using System.Windows.Media;
using WaveFunctionCollapseCore;

namespace Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int k_SceneWidth = 64;
    private const int k_SceneHeight = 64;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Setup image to remove sizing of pixels (we want to see huge pixels)
        Image.Stretch = Stretch.Uniform;
        RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);

        //Rotate canvas by 180 deg (because in wfc "Above" (y++) results in going down on the canvas)
        Image.LayoutTransform = new RotateTransform(180);

        // Setup bitmap ang graphics
        var bitmap = new Bitmap(k_SceneWidth, k_SceneHeight);

        // Draw..
        var wfcCore = new Core(k_SceneWidth, k_SceneHeight);
        void DrawingMethod(DrawRequest r)
        {
            bitmap.SetPixel(r.X, r.Y, r.Color);
            Dispatcher.Invoke(() =>
            {
                Image.Source = bitmap.ToImageSource();
            });
        }
        wfcCore.CollapseAndDrawIteratively(DrawingMethod);
    }
}