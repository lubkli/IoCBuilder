using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoCBuilder.Interception.Tests.Stubs
{
    public class ClassA
    {
        public virtual int CanDo()
        {
            return 1;
        }
    }

    public class ClassB : ClassA
    {
        public override int CanDo()
        {
            return 2;
        }
    }

    public class ClassC : ClassB
    {
        public override int CanDo()
        {
            return 3;
        }
    }
}
