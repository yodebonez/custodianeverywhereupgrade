using DataStore.context;
using DataStore.Irepository;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.repository
{
    public class store<T> where T : class
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private dbcontext session = null;
        private DbSet<T> _db = null;
        public store()
        {
            session = new dbcontext();
            _db = session.Set<T>();

        }

        


        public async Task<T> GetOne(int id)
        {
            T result = null;
            try
            {
                result = await _db.FindAsync(id);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                throw ex;
            }
            return result;
        }
        public async Task<List<T>> GetAll()
        {
            List<T> result = null;
            try
            {
                result = await _db.ToListAsync();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                throw ex;
            }

            return result;
        }

        public async Task<T> FindOneByCriteria(Func<T, bool> predicate)
        {
            T result = null;
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    result = _db.Where(predicate).FirstOrDefault();
                });

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                throw ex;
            }

            return result;
        }

        public async Task<List<T>> FindMany(Func<T, bool> predicate)
        {
            List<T> result = null;
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    result = _db.Where(predicate).ToList();
                });

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                throw ex;
            }

            return result;
        }

        public async Task<bool> Save(T entity)
        {
            bool result = false;
            try
            {
                _db.Add(entity);
                await session.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                throw ex;
            }
            return result;
        }

        public async Task<bool> Update(T entity)
        {
            bool result = false;
            try
            {
                session.Entry<T>(entity).State = EntityState.Modified;
                await session.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                throw ex;
            }
            return result;
        }

        public async Task<List<T>> CreateQuery(string sql)
        {
            List<T> result = null;
            try
            {
                var sql_query = await _db.SqlQuery(sql).ToListAsync();
                result = sql_query;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                throw ex;
            }
            return result;
        }

        public async Task<bool> Delete(T entity)
        {
            bool result = false;
            try
            {
                session.Entry<T>(entity).State = EntityState.Deleted;
                await session.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                throw ex;
            }
            return result;
        }
    }
}
