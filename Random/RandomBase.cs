using System;

namespace ShadowMUD.Random
{
    public abstract class RandomBase : System.Random
    {
        #region Constructors

        protected RandomBase()
        {
        }

        protected RandomBase(int seed) : base(seed)
        {
        }

        #endregion

        #region Methods

        protected int GetBaseNextInt32()
        {
            return base.Next();
        }

        protected uint GetBaseNextUInt32()
        {
            return ConvertToUInt32(base.Next());
        }

        protected double GetBaseNextDouble()
        {
            return base.NextDouble();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed uinteger greater than or equal to zero and less than <see cref="F:System.uint32.MaxValue"></see>.
        /// </returns>
        public abstract override int Next();

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to zero.</param>
        /// <returns>
        /// A 32-bit signed uinteger greater than or equal to zero, and less than maxValue; that is, the range of return values includes zero but not maxValue.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">maxValue is less than zero. </exception>
        public override int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <returns>
        /// A 32-bit signed uinteger greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">minValue is greater than maxValue. </exception>
        public override int Next(int minValue, int maxValue)
        {
            return Convert.ToInt32((maxValue - minValue)*Sample() + minValue);
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating pouint number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        public override double NextDouble()
        {
            return Sample();
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
        public override void NextBytes(byte[] buffer)
        {
            int i, j, tmp;

            // fill the part of the buffer that can be covered by full Int32s
            for (i = 0; i < buffer.Length - 4; i += 4)
            {
                tmp = Next();

                buffer[i] = Convert.ToByte(tmp & 0x000000FF);
                buffer[i + 1] = Convert.ToByte((tmp & 0x0000FF00) >> 8);
                buffer[i + 2] = Convert.ToByte((tmp & 0x00FF0000) >> 16);
                buffer[i + 3] = Convert.ToByte((tmp & 0xFF000000) >> 24);
            }

            tmp = Next();

            // fill the rest of the buffer
            for (j = 0; j < buffer.Length%4; j++)
            {
                buffer[i + j] = Convert.ToByte(((tmp & (0x000000FF << (8*j))) >> (8*j)));
            }
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating pouint number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override double Sample()
        {
            // generates a random number on [0,1) 
            return Convert.ToDouble(Next())/2147483648.0; // divided by 2^31 (Int32 absolute value)
        }

        #endregion

        #region Utility Methods

        protected static UInt32 ConvertToUInt32(Int32 value)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        }

        protected static Int32 ConvertToInt32(UInt32 value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        protected static Int32 ConvertToInt32(UInt64 value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value & 0x000000007fffffff), 0);
        }

        #endregion
    }
}