using System;

namespace ShadowMUD.Random
{
    public class Minstd : RandomBase
    {
        #region Constructors

        public Minstd() : this(Convert.ToInt32(DateTime.Now.Ticks & 0x000000007FFFFFFF))
        {
        }

        public Minstd(int seed)
            : base(seed)
        {
            _i = GetBaseNextInt32();
        }

        #endregion

        #region Member Variables

        private const int A = 48271; // multiplier (16807 also works);
        private const int M = 2147483647; // modulus 2^31-1, a prime
        private const int Q = M/A; // m / a
        private const int R = M%A; // m % a

        private int _i;

        #endregion

        public override int Next()
        {
            #region Execution

            _i = A*(_i%Q) - R*(_i/Q);

            if (_i < 0)
                _i = _i + M;

            return (_i);

            #endregion
        }
    }
}