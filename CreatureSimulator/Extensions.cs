using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatureSimulator
{
    public static class Extensions
    {
        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> list)
        {
            return list.Where(x => x != null);
        } 
    }
}
