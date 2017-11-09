using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace lens2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: ModernWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        async void rec_button_Click(object sender, RoutedEventArgs e)
        {
            rec_button.IsEnabled = false;

            var predictedOutput = predictOutput();    
            var outputControlVector = new Rectangle[] { softRec, noneRec, hardRec };
            var outputLabelControlVector = new TextBlock[] { softTextBlock, noneTextBlock, hardTextBlock };
            var task_xs = new Task[predictedOutput.Length];

            await Task.WhenAll(fmap(0,
                (value, pl, pb) =>
                    progress(0, 1, (a, b) => a - b, Tuple.Create(pl, pb)),
                task_xs, predictedOutput, outputLabelControlVector, outputControlVector));

            await Task.WhenAll(fmap(0,
                (value, pl, pb) =>
                    progress((int)value, 1, (a, b) => a + b, Tuple.Create(pl, pb)),
                task_xs, predictedOutput, outputLabelControlVector, outputControlVector));

            rec_button.IsEnabled = true;
        }

        /// <summary>
        /// Predict the output based on selected input indexes
        /// </summary>
        /// <returns>Predicted output vector</returns>
        double[] predictOutput ()
        {
            var twoClasses = new int[] { 1, -1 };
            var predicted_value_xs = Predictor.predict(
                x => x < 0 ? 0 : Math.Round(100 * x),
                new double[] {
                        new int[] { 1, 0, -1 } [ age_comboBox.SelectedIndex ],
                        twoClasses [ spec_perscrip_label_comboBox.SelectedIndex ],
                        twoClasses [ astigmatism_comboBox.SelectedIndex ],
                        twoClasses [ tear_production_rate_comboBox.SelectedIndex ] });
            return predicted_value_xs;
        }

        TextBlock changePercentageContent(double value, TextBlock tb)
        {
            tb.Text = value + "%";
            return tb;
        }

        Rectangle changeWidth(double width, Rectangle rec)
        {
            rec.Width = width;
            return rec;
        }

        async Task<Tuple<TextBlock, Rectangle>> progress(int max, int delay,
            Func<double, double, double> delta,
            Tuple<TextBlock, Rectangle> progressElems)
        {
            if (progressElems.Item2.Width.Equals(max))
            {
                return progressElems;
            }

            await Task.Delay(delay);

            var delta_val = delta(progressElems.Item2.Width, 1);
            var progressedLabel = changePercentageContent(delta_val, progressElems.Item1);
            var progressedRec = changeWidth(delta_val, progressElems.Item2);

            return await progress(max, delay, delta, Tuple.Create(progressedLabel, progressedRec));
        }

        Task[] fmap(int i,
            Func<double, TextBlock, Rectangle, Task> mapper,
            Task[] acc, double[] d_xs,
            TextBlock[] pl_xs, Rectangle[] pb_xs)
        {
            if (i > (acc.Length - 1)) { return acc; }
            acc[i] = mapper(d_xs[i], pl_xs[i], pb_xs[i]);
            return fmap((i + 1), mapper, acc, d_xs, pl_xs, pb_xs);
        }
    }
}