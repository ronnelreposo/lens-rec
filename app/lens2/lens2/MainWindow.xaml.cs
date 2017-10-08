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

            var twoClasses = new int[] { 1, -1 };
            var predicted_value_xs = Predictor.predict(
                x => x < 0 ? 0 : Math.Round(100 * x),
                new double[] {
                        new int[] { 1, 0, -1 } [ age_comboBox.SelectedIndex ],
                        twoClasses [ spec_perscrip_label_comboBox.SelectedIndex ],
                        twoClasses [ astigmatism_comboBox.SelectedIndex ],
                        twoClasses [ tear_production_rate_comboBox.SelectedIndex ] });
            var rec_xs = new Rectangle[] { soft_rec, none_rec, hard_rec };
            var tb_xs = new TextBlock[] { soft_tb, none_tb, hard_tb };
            var task_xs = new Task[predicted_value_xs.Length];

            Func<double, double, bool> condition = (a, b) => a == b;

            await Task.WhenAll(fmap(0,
                (value, pl, pb) =>
                    progress(0, 1, condition, (a, b) => a - b, Tuple.Create(pl, pb)),
                task_xs, predicted_value_xs, tb_xs, rec_xs));

            await Task.WhenAll(fmap(0,
                (value, pl, pb) =>
                    progress((int)value, 1, condition, (a, b) => a + b, Tuple.Create(pl, pb)),
                task_xs, predicted_value_xs, tb_xs, rec_xs));

            rec_button.IsEnabled = true;
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
            Func<double, double, bool> cond,
            Func<double, double, double> delta,
            Tuple<TextBlock, Rectangle> progressElems)
        {
            if (cond(progressElems.Item2.Width, max))
            {
                return progressElems;
            }

            await Task.Delay(delay);

            var delta_val = delta(progressElems.Item2.Width, 1);
            var progressedLabel = changePercentageContent(delta_val, progressElems.Item1);
            var progressedRec = changeWidth(delta_val, progressElems.Item2);

            return await progress(max, delay, cond, delta, Tuple.Create(progressedLabel, progressedRec));
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