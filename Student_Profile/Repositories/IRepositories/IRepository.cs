using System.Linq.Expressions;

namespace Student_Profile.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task<bool> CreateAsync(T entity);

        Task<bool> EditAsync(T entity);

        Task<bool> DeleteAsync(T entity);

        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null, bool tracked = true);

        T? GetOne(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null, bool tracked = true);
    }

}
