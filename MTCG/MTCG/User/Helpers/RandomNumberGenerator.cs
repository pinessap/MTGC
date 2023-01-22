using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.User.Helpers
{
    static class RandomNumberGenerator
    {
        private static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

        public static int Next()
        {
            return Random.Value.Next();
        }

        public static int Next(int maxValue)
        {
            return Random.Value.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return Random.Value.Next(minValue, maxValue);
        }
    }
}
