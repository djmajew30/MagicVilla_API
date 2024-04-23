﻿using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    //using T instead of Villa for generic
    public class Repository<T> : IRepository<T> where T : class
    {
        //add applicationDBContext
        private readonly ApplicationDbContext _db; //ctrl + . generate constructor ctor
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            //_db.VillaNumbers.Include(u => u.Villa).ToList();
            this.dbSet = _db.Set<T>();
        }

        //add async to method
        public async Task CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            //73 include villa when igetting villa number. 
            //string? includeProperties = null accepts a string, such as "Villa,VillaSpecial", so that is the format we must use
            //
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();
        }


        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null
            //117. Pagination (int pageSize = 3, int pageNumber = 1)
            , int pageSize = 3, int pageNumber = 1)
        {
            IQueryable<T> query = dbSet; //does not get executed right away, so we can add filter

            if (filter != null)
            {
                query = query.Where(filter);
            }

            //117. Pagination
            if (pageSize > 0)
            {
                //set max to 100
                if (pageSize > 100)
                {
                    pageSize = 100;
                }

                //which records to get
                //skip0.take(5)
                //page number- 2     || page size -5
                //skip(5*(1)) take(5)
                query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }

            //73 include villa when getting villa number. 
            //string? includeProperties = null accepts a string, such as "Villa,VillaSpecial", so that is the format we must use
            //
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.ToListAsync();
        }

        public async Task RemoveAsync(T entity)
        {
            dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }


    }
}
