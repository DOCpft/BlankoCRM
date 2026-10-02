using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;

namespace BlankoCRM.UnitTests.Authentication;

internal sealed class AsyncQueryable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public AsyncQueryable(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public AsyncQueryable(Expression expression) : base(expression)
    {
    }

    IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken) =>
        new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new AsyncQueryProvider<T>(this);
}

internal sealed class AsyncQueryProvider<TEntity>(IQueryProvider innerProvider) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        var elementType = expression.Type.GetGenericArguments().First();
        var query = (IQueryable)Activator.CreateInstance(typeof(AsyncQueryable<>).MakeGenericType(elementType), expression)!;
        return query;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new AsyncQueryable<TElement>(expression);

    public object? Execute(Expression expression) => innerProvider.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => innerProvider.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments().Single();
        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .Single(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
            .MakeGenericMethod(expectedResultType)
            .Invoke(innerProvider, [expression]);
        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, [executionResult])!;
    }
}

internal sealed class AsyncEnumerator<T>(IEnumerator<T> innerEnumerator) : IAsyncEnumerator<T>
{
    public T Current => innerEnumerator.Current;

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(innerEnumerator.MoveNext());

    public ValueTask DisposeAsync()
    {
        innerEnumerator.Dispose();
        return ValueTask.CompletedTask;
    }
}
