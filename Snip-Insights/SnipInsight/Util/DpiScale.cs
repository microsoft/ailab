// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace SnipInsight.Util
{
    public class DpiScale : IEquatable<DpiScale>
    {
        private readonly double _x;
        private readonly double _y;

        public DpiScale()
            : this(1, 1)
        {

        }

        public DpiScale(double value)
            : this(value, value)
        {

        }

        public DpiScale(double x, double y)
        {
            if (double.IsNaN(x) || x <= 0)
                throw new ArgumentOutOfRangeException("x");

            if (double.IsNaN(y) || y <= 0)
                throw new ArgumentOutOfRangeException("y");

            this._x = x;
            this._y = y;
        }

        public double X
        {
            get { return _x; }
        }

        public double Y
        {
            get { return _y; }
        }

        public override string ToString()
        {
            return "X=" + X.ToString("0.###") + ", Y=" + Y.ToString("0.###");
        }

        #region Equatable

        public sealed override bool Equals(object obj)
        {
            if (obj is DpiScale)
            {
                return Equals((DpiScale)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() << 2;
        }

        public bool Equals(DpiScale other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        #endregion
    }
}
