using System;

namespace ShadowMUD.Random
{
    /// <summary>
    /// <para>
    /// Based on subtractive generalized Fibonacci sequence
    /// </para>
    /// <remarks> see: http://www.shadlen.org/ichbin/random/generators.htm#knuth</remarks>
    /// </summary>
    public sealed class Knuth : RandomBase
    {
        #region Constructors

        public Knuth() : this(Convert.ToInt32(DateTime.Now.Ticks & 0x000000007FFFFFFF))
        {
        }

        public Knuth(int seed)
            : base(seed)
        {
            #region Declarations

            int i;

            #endregion

            #region Initialization

            _mX = new int[N];
            var v = seed;
            var w = 1;

            #endregion

            #region Execution

            for (i = 0; i < N; i++)
            {
                var j = (21*i)%N;

                _mX[j] = w;

                w = v - w;

                if (w < 1)
                    w = w + (m - 1);

                v = _mX[j];
            }

            // set the pointers
            _mP = N - 1;
            _mQ = N - M - 1;

            // prime the pump
            for (i = 0; i < 3*N; i++)
            {
                Next();
            }

            #endregion
        }

        #endregion

        #region Member Variables

        private const int m = 2147483647; // 2^31 - 1
        private const int M = 24;
/*
        private const double Mi = 1.0/m;
*/
        private const int N = 55;

        private readonly int[] _mX;
        private int _mP, _mQ;

        #endregion

        #region Methods

        public override int Next()
        {
            #region Declarations

            #endregion

            #region Execution

            if (_mP == N - 1)
            {
                _mP = 0;
            }
            else
            {
                (_mP)++;
            }

            if (_mQ == N - 1)
            {
                _mQ = 0;
            }
            else
            {
                (_mQ)++;
            }

            var y = _mX[_mP] - _mX[_mQ];

            if (y < 1)
            {
                y = y + (m - 1);
            }

            _mX[_mP] = y;

            return y;

            #endregion
        }

        #endregion
    }
}