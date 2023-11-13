using System;
using System.Collections.Generic;
using System.Text;

namespace Penqueen.Types
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class DeclareCollectionAttribute : Attribute
    {
    }
}
