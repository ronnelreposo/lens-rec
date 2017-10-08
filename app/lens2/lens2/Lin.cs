using System;

namespace lens2
{
    internal static class Lin
    {
        internal static double[] opxs(Func<double, double> f, int i, double[] acc, double[] xs)
        {
            if (i > (xs.Length - 1)) { return acc; }
            else
            {
                acc[i] = f(xs[i]);
                return opxs(f, (i + 1), acc, xs);
            }
        }

        internal static double[] opxs2(Func<double, double, double> f, int i, double[] acc, double[] xs, double[] ys)
        {
            if (xs.Length != ys.Length)
            {
                throw new Exception("Vectors are not in same length.");
            }

            if (i > (xs.Length - 1)) { return acc; }
            else
            {
                acc[i] = f(xs[i], ys[i]);
                return opxs2(f, (i + 1), acc, xs, ys);
            }
        }

        internal static double[] mapxs(Func<double, double> f, double[] vector)
            => opxs(f, 0, new double[vector.Length], vector);

        internal static double[] mulxs(double[] xs, double[] ys)
            => opxs2(((x, y) => x * y), 0, (new double[xs.Length]), xs, ys);

        internal static double[] addxs(double[] xs, double[] ys)
            => opxs2(((x, y) => x + y), 0, (new double[xs.Length]), xs, ys);
    }
}