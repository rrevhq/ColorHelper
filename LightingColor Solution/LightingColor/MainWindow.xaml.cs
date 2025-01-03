using System.Windows;
using System.Windows.Media;
using ColorMine.ColorSpaces;

namespace LightingColor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var solid = new Hex(txtSolid.Text).To<Hsl>();
            var light = new Hex(txtLight.Text).To<Hsl>();
            rectSolid.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(txtSolid.Text));
            rectLight.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(txtLight.Text));
            LinearGradientBrush linear = new LinearGradientBrush();
            linear.StartPoint = new Point(0, 0);
            linear.EndPoint = new Point(1, 0);
            linear.GradientStops = new GradientStopCollection();
            int cut = 10;
            for (int i = 0; i < cut; i++)
            {
                var offset = 1d / cut * i;
                var hsl = BlendHSL(solid, light, offset, double.Parse(txtShreshold.Text));
                var color = hsl.ToSystemColor();
                linear.GradientStops.Add(new GradientStop
                {
                    Offset = offset,
                    Color = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B)
                }); ;
            }
            rect.Fill = linear;

        }

        public Hsl BlendHSL(Hsl a, Hsl b, double intensity, double threshold)
        {
            double offset = (intensity - threshold) / (1 - threshold);

            // 解构输入颜色
            double hA = a.H, sA = a.S, lA = a.L;
            double hB = b.H, sB = b.S, lB = b.L;

            // 线性插值 Lightness 和 Saturation
            double lResult = intensity * lB; //LinearInterpolation(threshold, 1, lA, lB, offset);

            // 色相插值（考虑角度循环问题）
            double hResult;
            if (Math.Abs(hA - hB) > 180)
            {
                // 如果两个角度差大于 180，跨越了 0°/360°，调整方向
                if (hA > hB) hB += 360;
                else hA += 360;
            }
            hResult = ((1 - intensity) * hA + intensity * hB) % 360;

            // 判断色相差别
            double hueDifference = Math.Abs(hA - hB);
            double sResult = 0;
            if (intensity < threshold)
            {
                if (hueDifference < 90)
                {
                    // 光强越低，饱和度越高
                    sResult = LinearInterpolation(0, threshold, 1, sA, intensity);
                }
                else
                {
                    // 光强越低，饱和度越低
                    sResult = LinearInterpolation(0, threshold, 0, sA, intensity);
                }
            }
            else
            {
                // 强光区时，趋近与光色
                sResult = LinearInterpolation(threshold, 1, sA, sB, intensity);
            }

            // 返回混合后的 HSL 颜色
            var c = new Hsl();
            c.H = hResult;
            c.S = sResult;
            c.L = lResult;
            return c;
        }

        public double LinearInterpolation(double x1, double x2, double y1, double y2, double x)
        {
            if (x == x1)
            {
                return y1;
            }

            if (x2 == x1)
            {
                throw new ArgumentNullException("x2 is supposed to be different from x1.");
            }

            return y1 + (y2 - y1) / (x2 - x1) * (x - x1);
        }
    }
}