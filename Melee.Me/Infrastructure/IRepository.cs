using System;
using System.Linq;
using System.Linq.Expressions;

namespace Melee.Me.Infrastructure
{
    public interface IRepository<T>
    {
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
    }
}
