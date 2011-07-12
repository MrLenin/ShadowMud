using System;

namespace ShadowMUD.Random
{
    /// <summary>
    /// <para>
    /// An implementation of the Generalized Feedback Shift Register (GFSR) R250_521 pseudo random number generator
    /// </para>
    /// <remarks>
    /// see: 
    /// http://portal.acm.org/citation.cfm?id=321765.321777&coll=GUIDE&dl=ACM&idx=J401&part=periodical&WantType=periodical&title=Journal%20of%20the%20ACM%20(JACM)
    /// </remarks>
    /// </summary>
    public sealed class R250521 : RandomBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:R250521"/> class.
        /// </summary>
        public R250521() : this(Convert.ToInt32(DateTime.Now.Ticks & 0x000000007FFFFFFF))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:R250521"/> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public R250521(int seed) : base(seed)
        {
            #region Declarations

            // Convert.ToUInt32(0x7FFFFFFF)  ==	2147483647U 

            #endregion

            #region Execution

            _r250Buffer = new uint[250];
            _r521Buffer = new uint[521];
            var i = 521U;
            var mask1 = 0x01U;
            var mask2 = 2147483647U;

            while (i-- > 250)
            {
                _r521Buffer[i] = GetBaseNextUInt32();
            }

            while (i-- > 31)
            {
                _r250Buffer[i] = GetBaseNextUInt32();
                _r521Buffer[i] = GetBaseNextUInt32();
            }

            // Establish linear independence of the bit columns
            // by setting the diagonal bits and clearing all bits above

            while (i-- > 0)
            {
                _r250Buffer[i] = (GetBaseNextUInt32() | mask1) & mask2;
                _r521Buffer[i] = (GetBaseNextUInt32() | mask1) & mask2;
                mask2 ^= mask1;
                mask1 >>= 1;
            }

            _r250Buffer[0] = mask1;
            _r521Buffer[0] = mask2;
            _r250Index = 0;
            _r521Index = 0;

            #endregion
        }

        #endregion

        #region Member Variables

        private readonly uint[] _r250Buffer;
        private readonly uint[] _r521Buffer;
        private int _r250Index, _r521Index;

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed uinteger greater than or equal to zero and less than <see cref="F:System.uint32.MaxValue"></see>.
        /// </returns>
        public override int Next()
        {
            #region Declarations

            int i1, i2, j1, j2;
            uint r, s;

            #endregion

            #region Execution

            i1 = _r250Index;
            i2 = _r521Index;

            j1 = i1 - 146; // (249 - 103)

            if (j1 < 0)
                j1 = i1 + 103;

            j2 = i2 - 352; // (520 - 168)

            if (j2 < 0)
                j2 = i2 + 167;

            r = _r250Buffer[j1] ^ _r250Buffer[i1];
            _r250Buffer[i1] = r;

            s = _r521Buffer[j2] ^ _r521Buffer[i2];
            _r521Buffer[i2] = s;

            i1 = (i1 == 249) ? 0 : (i1 + 1);
            _r250Index = i1;

            i2 = (i2 == 520) ? 0 : (i2 + 1);
            _r521Index = i2;

            return ConvertToInt32(r ^ s);

            #endregion
        }

        #endregion
    }
}