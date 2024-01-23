using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Irepository
{
    public interface Istore<T> where T : class
    {
        Task<T> GetOne(int id);
        Task<List<T>> GetAll();
        Task<T> FindOneByCriteria(Func<T, bool> predicate);
        Task<List<T>> FindMany(Func<T, bool> predicate);
        Task<bool> Update(T entity);
        Task<bool> Save(T entity);
    }
}
