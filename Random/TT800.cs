using System;

namespace ShadowMUD.Random
{
    /// <summary>
    /// implements the tGFSR based on
    /// x_n = ( x_(n-N) >> 1 ) ^ x_(n-M)
    /// plus an extra twist,
    /// which has period 2^(32*N) - 1
    /// <remarks>
    /// see: http://www.shadlen.org/ichbin/random/generators.htm#tt800
    /// </remarks>
    /// </summary>
    public class TT800 : RandomBase
    {
        #region Constructors

        public TT800() : this(Convert.ToInt32(DateTime.Now.Ticks & 0x000000007FFFFFFF))
        {
        }

        public TT800(int seed)
            : base(seed)
        {
            int i;

            _x = new uint[N];

            var xx = new uint[]
                            {
                                0x95f24dab, 0x0b685215, 0xe76ccae7, 0xaf3ec239, 0x715fad23,
                                0x24a590ad, 0x69e4b5ef, 0xbf456141, 0x96bc1b7b, 0xa7bdf825,
                                0xc1de75b7, 0x8858a9c9, 0x2da87693, 0xb657f9dd, 0xffdc8a9f,
                                0x8121da71, 0x8b823ecb, 0x885d05f5, 0x4e20cd47, 0x5a9ad5d9,
                                0x512c0c03, 0xea857ccd, 0x4cc1d30f, 0x8891a8a1, 0xa6b7aadb
                            };


            for (i = 0; i < N; i++)
            {
                _x[i] = xx[i] ^ GetBaseNextUInt32();
            }

            _p = N - 1;
            _q = N - M - 1;
        }

        #endregion

        #region Member Variables

        private const uint A = 0x8ebfd028U;
        private const uint B = 0x2b5b2500U;
        private const uint C = 0xdb8b0000U;
        private const int L = 16;
        private const int M = 18;
        private const int N = 25;
        private const int S = 7;
        private const int T = 15;

        private readonly uint[] _x;
        private int _p, _q;

        #endregion

        #region Methods

        public override int Next()
        {
            #region Declarations

            #endregion

            #region Execution

            if (_p == N - 1)
            {
                _p = 0;
            }
            else
            {
                (_p)++;
            }

            if (_q == N - 1)
            {
                _q = 0;
            }
            else
            {
                (_q)++;
            }

            var z = _x[(_p)];
            var y = _x[(_q)] ^ (z >> 1);

            if (z%2 != 0)
            {
                y ^= A;
            }

            if (_p == N - 1)
            {
                _x[0] = y;
            }
            else
            {
                _x[(_p) + 1] = y;
            }

            y ^= ((y << S) & B);
            y ^= ((y << T) & C);
            y ^= (y >> L); // improves bits

            return ConvertToInt32(y & 0x7FFFFFFF);

            #endregion
        }

        #endregion
    }
}