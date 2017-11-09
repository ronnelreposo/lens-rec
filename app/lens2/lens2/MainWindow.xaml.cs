using FirstFloor.ModernUI.Windows.Controls;
using lens2.Ext;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Reactive.Linq;
using System.Reactive;

namespace lens2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow ()
        {
            InitializeComponent();

            var sRecommendButtonClick = rec_button.StreamClickEvent();
            sRecommendButtonClick.Subscribe(button =>
            {
                rec_button_Click(null, null);
            });
        }

        async void rec_button_Click (object sender, RoutedEventArgs e)
        {
            rec_button.IsEnabled = false;

            var predictedOutput = predictOutput();
            var outputRectangleControlVector = new Rectangle[] { softRec, noneRec, hardRec };
            var outputTextBlockControlVector = new TextBlock[] { softTextBlock, noneTextBlock, hardTextBlock };

            var clearedRectangleControlVector = from rectangle
                                             in outputRectangleControlVector
                                             select resetWidth(rectangle);

            var clearedTextBlockControlVector = from textblock
                                                in outputTextBlockControlVector
                                                select changeText(textblock);

            var progressed = progressOutputToControls(
                clearedRectangleControlVector.ToArray(),
                clearedTextBlockControlVector.ToArray(),
                predictedOutput);

            await Task.WhenAll(progressed);

            rec_button.IsEnabled = true;
        }

        /// <summary>
        /// Progresses the output value to output controls.
        /// </summary>
        /// <param name="rectangleControlVector">Rectangle output control, used as a progress bar.</param>
        /// <param name="textBlockControlVector">TextBlock output control, used to view percentage.</param>
        /// <param name="valueVector">The values to be progressed.</param>
        /// <returns>Unit</returns>
        Task<Unit>[] progressOutputToControls (Rectangle[] rectangleControlVector, TextBlock[] textBlockControlVector, double[] valueVector)
        {
            var outputControlsVector = rectangleControlVector
                .Zip(textBlockControlVector,
                (rectangle, textBlock) =>
                new {
                    Rectangle = rectangle,
                    TextBlock = textBlock
                });

            var outputControlsVectorWithValue = valueVector
                .Zip(outputControlsVector,
                (value, outputControls) =>
                new {
                    Value = value,
                    Rectangle = outputControls.Rectangle,
                    TextBlock = outputControls.TextBlock
                });

            var progressed = outputControlsVectorWithValue
                .Select(valuedOutputControls =>
                ProgressAsync((int) valuedOutputControls.Value, valuedOutputControls.Rectangle, valuedOutputControls.TextBlock, EaseInOutCubic));

            return progressed.ToArray();
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

        /// <summary>
        /// Changes the width of the rectangle given the width parameter.
        /// </summary>
        /// <param name="width">The given width</param>
        /// <param name="rec">The Rectangle control</param>
        /// <returns>The changed rectangle width</returns>
        Rectangle changeWidth(double width, Rectangle rec)
        {
            rec.Width = width;
            return rec;
        }

        /// <summary>
        /// Resets the width of the Rectangle to its default width.
        /// </summary>
        /// <param name="rec">The given rectangle.</param>
        /// <param name="defaultWidth">The Default width of the rectangle, default value of 0.</param>
        /// <returns>The reseted rectangle width.</returns>
        Rectangle resetWidth(Rectangle rec, int defaultWidth = 0) => changeWidth(defaultWidth, rec);

        /// <summary>
        /// Ease In-Out Cubic Function.
        /// </summary>
        readonly Func<double, double> EaseInOutCubic = x => ( x < 0.5 ) ? 4 * x * x * x : ( x - 1 ) * ( 2 * x - 2 ) * ( 2 * x - 2 ) + 1;

        /// <summary>
        /// Normalize the value from 0 to 1.
        /// </summary>
        /// <param name="value">The given value to be normalized.</param>
        /// <param name="max">The given maximum value.</param>
        /// <param name="min">The given minimum value.</param>
        /// <returns>scaled/normalized value.</returns>
        double minmax (double value, double max, double min = 0) => ( min.Equals(0) ) ? ( value / max ) : ( ( value - min ) / ( max - min ) );

        /// <summary>
        /// Changes the TextBlock Text with the given value.
        /// </summary>
        /// <param name="textblock">The Textblock to which the text is to be changed.</param>
        /// <param name="value">The given string value. (Default: empty string)</param>
        /// <returns>Textblock changed value.</returns>
        private TextBlock changeText (TextBlock textblock, string value = "")
        {
            textblock.Text = value;
            return textblock;
        }

        /// <summary>
        /// Changes the text of the TextBlock with the given value,
        /// plus the "%" to indicate its percentage.
        /// </summary>
        /// <param name="value">The given value</param>
        /// <param name="tb">The given TextBlock control</param>
        /// <returns>TextBlock changed value.</returns>
        TextBlock changePercentageContent(double value, TextBlock tb) => changeText(tb, value + "%");

        /// <summary>
        /// Progresses the output (Asyncronously).
        /// It uses Rectangle and Textbox to project the maximum value.
        /// </summary>
        /// <param name="max">The maximum value to be projected.</param>
        /// <param name="rectangle">The Rectangle control.</param>
        /// <param name="textblock">The TextBlock control.</param>
        /// <param name="delta">The change function. (Used for animation)</param>
        /// <param name="i">The step index. Zero by default.</param>
        /// <param name="delay">The step delay. 3 seconds by default.</param>
        /// <returns>Unit</returns>
        async Task<Unit> ProgressAsync (int max, Rectangle rectangle, TextBlock textblock, Func<double, double> delta, int i = 0, int delay = 3)
        {
            if ( i.Equals(max) ) { return Unit.Default; }
            else
            {
                var normalizedInput = minmax(i, max);
                var i_ = delta(normalizedInput);
                var width_ = Math.Round(i_ * max);
                var rectangleDelta = changeWidth(width_, rectangle);
                var textBlockDelta = changePercentageContent(width_, textblock);

                await Task.Delay(delay);

                return await ProgressAsync(max, rectangleDelta, textblock, delta, ( i + 1 ));
            }
        }
    }
}