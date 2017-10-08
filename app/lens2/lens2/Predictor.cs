using System;
using System.Linq;

namespace lens2
{
    internal static class Predictor
    {
        static double[] ws(int i, double[] acc, double[] i_xs, double[][] ws_xs)
        {
            if (i > (ws_xs.Length - 1)) { return acc; }
            acc[i] = Lin.mulxs(i_xs, ws_xs[i]).Sum();
            return ws((i + 1), acc, i_xs, ws_xs);
        }

        static double[] ws(double[] i_xs, double[][] ws_xss, double[] b_xs)
            => Lin.addxs(b_xs, (ws(0, (new double[ws_xss.Length]), i_xs, ws_xss)));

        static double[] ff(
            Func<double[], double[][], double[], double[]> f,
            Func<Func<double, double>, double[], double[]> fmap,
            double[] inputs)
            => fmap(Math.Tanh, f(fmap(Math.Tanh, f(inputs,
                new double[][] {
                new double[] { 3.831233387, 4.01329833, 6.296136811, 6.112385545 },
                new double[] { -4.343587541, 2.251556646, 6.595788413, -6.756682305 }
            }, new double[] { -10.4525315, 8.912132427 })),
                new double[][] {
                new double[] { -0.00339809083, -1.34502627 },
                new double[] { -1.44093948, 1.43240389 },
                new double[] { 1.307363047, -0.001861278349 }
            }, new double[] { 1.336714147, -0.01862609591, 1.303547049 }));

        internal static double[] predict(Func<double, double> percentConv, double[] inp_xs)
            => Lin.mapxs(percentConv, ff(ws, Lin.mapxs, inp_xs));
    }
}