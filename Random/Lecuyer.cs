using System;

namespace ShadowMUD.Random
{
    /// <summary>
    /// <para>
    /// define two MCGs both implementable with Schrage's method on a 32-bit word
    /// </para>
    /// <remarks>
    /// see: http://www.shadlen.org/ichbin/random/generators.htm#lecuyer
    /// </remarks>
    /// </summary>
    public sealed class Lecuyer : RandomBase
    {
        #region Constructors

        public Lecuyer() : this(Convert.ToInt32(DateTime.Now.Ticks & 0x000000007FFFFFFF))
        {
        }

        public Lecuyer(int seed)
            : base(seed)
        {
            _x1 = seed%M1;
            _x2 = seed%M2;
        }

        #endregion

        #region Member Variables

        private const int A1 = 26756; // 2^2 * 6689

        private const int A2 = 30318; // 2 * 3 * 31 * 163
        private const int M1 = 2147483647; // 2^31 - 1, a prime
        private const int M2 = 2145483479; // 2^31 - 2000169, a prime
        private const int Q1 = M1/A1; // m / a
        private const int Q2 = M2/A2; // m / a
        private const int R1 = M1%A1; // m % a
        private const int R2 = M2%A2; // m % a
        private int _x1, _x2;

        #endregion

        #region Methods

        public override int Next()
        {
            #region Declarations

            #endregion

            #region Execution

            // advance first generator
            _x1 = A1*(_x1%Q1) - R1*(_x1/Q1);

            if (_x1 < 0)
                _x1 += M1;

            // advance second generator
            _x2 = A2*(_x2%Q2) - R2*(_x2/Q2);

            if (_x2 < 0)
                _x2 += M2;

            // combine results
            var x = _x1 - _x2;

            if (x < 1)
                x += M1 - 1;

            return x;

            #endregion
        }

        #endregion
    }
}