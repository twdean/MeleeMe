using System;
using System.Linq;
using System.Linq.Expressions;
using MeleeMeDatabase;

namespace Melee.Me.Infrastructure
{
    public interface IEntity<T>
    {
        T Add(string id, string item);
        T Get(string id);
        void Save(MeleeMeEntities dbContext);
        void Delete(T entity);
        IQueryable Find(Expression<Func<T, bool>> predicate);
    }
}
