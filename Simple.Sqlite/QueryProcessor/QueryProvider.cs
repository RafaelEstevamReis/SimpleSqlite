using Simple.Sqlite.Extension;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Simple.Sqlite.QueryProcessor
{
    public class QueryProvider<TElement> : IQueryProvider
    {
        private ISqliteConnection connection;
        public QueryProvider(ISqliteConnection connection)
        {
            this.connection = connection;
        }

        public virtual IQueryable CreateQuery(Expression expression)
        {
            Type elementType = expression.Type;

            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(Queryable<>)
                                            .MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
        public virtual IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new Queryable<T>(this, expression);
        }

        object IQueryProvider.Execute(Expression expression) => Execute<object>(expression);
        T IQueryProvider.Execute<T>(Expression expression) => Execute<T>(expression);

        T Execute<T>(Expression expression)
        {
            return default;
        }
    }
}
