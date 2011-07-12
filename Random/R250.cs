using System;

namespace ShadowMUD.Random
{
    /// <summary>
    /// <para>
    /// implements the GFSR ]
    /// x_n = x_(n-P) ^ x_(n-Q)
    //// which has period 2^P - 1
    /// </para>
    /// <remarks>
    /// see: http://www.shadlen.org/ichbin/random/generators.htm#r250
    /// </remarks>
    /// </summary>
    public class R250 : RandomBase
    {
        #region Constructors

        public R250() : this(Convert.ToInt32(DateTime.Now.Ticks & 0x000000007FFFFFFF))
        {
        }

        public R250(int seed)
            : base(seed)
        {
            #region Declarations

            int i;

            #endregion

            #region Initialization

            _x = new uint[P];

            // get the memory we need for initialization
            var e = new uint[P];
            var a = new uint[2*P];

            for (i = 0; i < 7; i++)
                _x[i] = GetBaseNextUInt32();

            _p = Convert.ToInt32(P*GetBaseNextDouble());

            #endregion

            #region Execution

            // construct P linearly independent basis vectors
            for (i = 0; i < P; i++)
            {
                //mini_rand(s);
                var k = L*i/P;
                e[i] = GetBaseNextUInt32() << k; // zeros to right of bit k
                e[i] = e[i] | (0x01U << k); // and a one at bit k
            }

            // construct 2P-1 coefficient bits
            for (i = 0; i < P; i++)
            {
                if (GetBaseNextDouble() > 0.5)
                {
                    a[i] = 1;
                }
                else
                {
                    a[i] = 0;
                }
            }
            for (i = P; i < 2*P; i++)
            {
                a[i] = a[i - P] ^ a[i - Q];
            }

            // construct first P-1 entries (``matrix seed'') by
            // combining basis vectors according to coefficient bits
            for (i = 0; i < P; i++)
            {
                _x[i] = 0;

                for (var j = 0; j < P; j++)
                {
                    if (a[i + j] != 0)
                    {
                        _x[i] = _x[i] ^ e[j];
                    }
                }
            }

            // set pointer to last element
            _p = P - 1;

            #endregion
        }

        #endregion

        #region Member Variables

        private const int L = 32; // word length (32 or 3)
        private const int P = 250; // degree of larger term (250 or 7)
        private const int Q = 103; // degree of smaller term (103 or 4)

        private readonly uint[] _x;
        private int _p;

        //static const int N = 250;
        //static const int M = 147;

        #endregion

        #region Methods

        public override int Next()
        {
            #region Declarations

            uint ret;

            #endregion

            #region Initialization

            var newP = _p;

            #endregion

            #region Execution

            // advance pointer
            if (newP == P - 1)
            {
                newP = 0;
            }
            else
            {
                newP++;
            }

            // compute next value
            if (newP < Q)
            {
                ret = _x[newP] ^ _x[(newP - Q + P)];
            }
            else
            {
                ret = _x[newP] ^ _x[(newP - Q)];
            }

            // replace value and pointer and return
            _p = newP;
            _x[_p] = ret;

            return ConvertToInt32(ret & 0x7fffffffU);

            #endregion
        }

        #endregion
    }
}