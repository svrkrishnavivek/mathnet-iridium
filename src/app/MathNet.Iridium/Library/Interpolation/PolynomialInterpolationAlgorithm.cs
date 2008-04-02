#region Math.NET Iridium (LGPL) by Ruegg + Contributors
// Math.NET Iridium, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2002-2008, Christoph R�egg, http://christoph.ruegg.name
//
// Contribution: Numerical Recipes in C++, Second Edition [2003]
//               Handbook of Mathematical Functions [1965]
//						
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public 
// License along with this program; if not, write to the Free Software
// Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#endregion

using System;
using MathNet.Numerics;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Interpolation
{
    /// <summary>
    /// Lagrange Polynomial Interpolation using Neville's Algorithm.
    /// </summary>
    public class PolynomialInterpolationAlgorithm : IInterpolationAlgorithm
    {
        private SampleList samples;
        private int order;
        private int effectiveOrder;

        /// <summary>
        /// Create a polynomial interpolation algorithm with the given order.
        /// </summary>
        public PolynomialInterpolationAlgorithm(int order)
        {
            this.order = order;
            this.effectiveOrder = order;
        }

        /// <summary>
        /// Precompute/optimize the algoritm for the given sample set.
        /// </summary>
        public void Prepare(SampleList samples)
        {
            this.samples = samples;
            this.effectiveOrder = Math.Min(order, samples.Count);
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        public double Interpolate(double t)
        {
            double error;
            return Interpolate(t, out error);
        }

        /// <summary>
        /// Interpolate at point t and return the estimated error as error-parameter.
        /// </summary>
        public double Interpolate(double t, out double error)
        {
            if(samples == null)
                throw new InvalidOperationException(Resources.InvalidOperationNoSamplesProvided);

            int closestIndex;
            int offset = SuggestOffset(t, out closestIndex);
            double[] c = new double[effectiveOrder];
            double[] d = new double[effectiveOrder];
            int ns = closestIndex - offset;
            double den, ho, hp;
            double x = 0;
            error = 0;

            if(samples.GetT(closestIndex) == t)
                return samples.GetX(closestIndex);

            for(int i = 0; i < effectiveOrder; i++)
            {
                c[i] = samples.GetX(offset + i);
                d[i] = c[i];
            }

            x = samples.GetX(offset + ns--);
            for(int level = 1; level < effectiveOrder; level++)
            {
                for(int i = 0; i < effectiveOrder - level; i++)
                {
                    ho = samples.GetT(offset + i) - t;
                    hp = samples.GetT(offset + i + level) - t;
                    den = (c[i + 1] - d[i]) / (ho - hp);
                    d[i] = hp * den;
                    c[i] = ho * den;
                }
                error = (2 * (ns + 1) < (effectiveOrder - level) ? c[ns + 1] : d[ns--]);
                x += error;
            }

            return x;
        }

        private int SuggestOffset(double t, out int closestIndex)
        {
            closestIndex = Math.Max(samples.Locate(t), 0);
            int ret = Math.Min(Math.Max(closestIndex - (effectiveOrder - 1) / 2, 0), samples.Count - effectiveOrder);
            if(closestIndex < (samples.Count - 1))
            {
                double dist1 = Math.Abs(t - samples.GetT(closestIndex));
                double dist2 = Math.Abs(t - samples.GetT(closestIndex + 1));

                if(dist1 > dist2)
                {
                    closestIndex++;
                }
            }
            return ret;
        }

        /// <summary>
        /// Extrapolate at point t.
        /// </summary>
        public double Extrapolate(double t)
        {
            return Interpolate(t);
        }

        /// <summary>
        /// True if the alorithm supports error estimation.
        /// </summary>
        /// <remarks>
        /// Always true for this algorithm.
        /// </remarks>
        public bool SupportErrorEstimation
        {
            get { return true; }
        }
    }
}
