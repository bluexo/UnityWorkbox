using System;

namespace Arthas.Common
{
    [AttributeUsage(AttributeTargets.All,AllowMultiple = true, Inherited = false)]
    public class AuthorAttribute : Attribute {

        public string Name;
        public string Date;
    }
}