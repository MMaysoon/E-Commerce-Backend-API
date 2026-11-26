using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Common
{
    public sealed class PaginatedResult<TEntity>
    {
        public PaginatedResult(int pageIndex, int pageSize, int totalCount, IReadOnlyList<TEntity> data)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageIndex, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
            ArgumentNullException.ThrowIfNull(data);

            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = totalCount;
            Data = data;
        }

        public int PageIndex { get; }
        public int PageSize { get; }
        public int Count { get; }
        public IReadOnlyList<TEntity> Data { get; }
    }
}
