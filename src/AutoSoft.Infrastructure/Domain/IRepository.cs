﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AutoSoft.Infrastructure.Domain
{
    public interface IRepository<TEntity> where TEntity : IAggregateRoot
    {

    }
}
