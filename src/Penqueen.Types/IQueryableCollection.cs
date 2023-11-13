using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Penqueen.Collections;

public interface IQueryableCollection<T> : ICollection<T>, IQueryable<T>
{
    void Load();
}

