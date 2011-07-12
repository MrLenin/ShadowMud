using System;

namespace ShadowMUD.Random
{
    /// <summary>
    /// <para>
    /// very quick linear congruential generator which takes its modulus
    /// operation from the unflagged overflow of a 32-bit register
    /// </para>
    /// <remarks>
    /// see: http://www.shadlen.org/ichbin/random/generators.htm#quick
    /// </remarks>
    /// </summary>
    public class Quick : RandomBase
    {
        #region Constructors

        public Quick() : this(Convert.ToInt32(DateTime.Now.Ticks & 0x000000007FFFFFFF))
        {
        }

        public Quick(int seed)
            : base(seed)
        {
            _i = Convert.ToUInt64(GetBaseNextInt32());
        }

        #endregion

        #region Member Variables

        private const uint A = 1099087573;

        private ulong _i;

        #endregion

        #region Methods

        public override int Next()
        {
            #region Execution

            _i = A*_i; // overflow occurs here!
            return ConvertToInt32(_i);

            #endregion
        }

        #endregion
    }
}