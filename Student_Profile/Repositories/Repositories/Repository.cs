using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Student_Profile.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;

namespace Student_Profile.Repositories.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbSet<T> _dbSet;
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // CRUD
        public async Task<bool> CreateAsync(T entity)
        {
            try
            {

                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
                return false;

            }
        }

        public async Task<bool> EditAsync(T entity)
        {
            try
            {

                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
                return false;

            }
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            try
            {

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
                return false;

            }
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            IQueryable<T> entities = _dbSet;

            if (expression is not null)
            {
                entities = entities.Where(expression);
            }

            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    entities = entities.Include(item);
                }
            }

            if (!tracked)
            {
                entities = entities.AsNoTracking();
            }

            return (await entities.ToListAsync());
        }

        public T? GetOne(Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            return GetAsync(expression, includes, tracked).GetAwaiter().GetResult().FirstOrDefault();
        }
    }
}
