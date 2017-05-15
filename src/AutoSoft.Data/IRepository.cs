﻿using AutoSoft.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AutoSoft.Data
{
    public interface IRepository<TEntity, TKeyType> 
        where TEntity : IEntity<TKeyType>
        where TKeyType : class
    {
        IList<TEntity> FindAll();

        IList<TEntity> FindBy(Expression<Func<TEntity, bool>> condition);

        TEntity FindById(TKeyType id);

        TEntity Add(TEntity entity);

        TEntity Update(TEntity entity);

        void Delete(TEntity entity);

        void Save();
    }
}
