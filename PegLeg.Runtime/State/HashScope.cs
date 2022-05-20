using PegLeg.Runtime.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PegLeg.Runtime.State
{
    public static class HashScope
    {
        public static HashValue Create()
        {
            Hasher hasher = new();
            hasher.Append(Interlocked.Increment(ref _nextScope));
            return hasher.GetAndReset();
        }

        private static int _nextScope;
    }
}
