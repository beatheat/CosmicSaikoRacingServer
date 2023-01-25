using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSRServer
{
    internal class Resource
    {
        public enum Type
        {
            FOSSIL, ELECTRIC, BIO, NUCLEAR, COSMIC
        }

        Type type;
    }
}
