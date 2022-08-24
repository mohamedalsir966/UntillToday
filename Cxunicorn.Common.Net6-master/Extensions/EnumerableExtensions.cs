using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cxunicorn.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> AsBatches<T>(this IEnumerable<T> sourceCollection, int batchSize)
        {
            _ = sourceCollection ?? throw new ArgumentNullException(nameof(sourceCollection));
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            var buffer = new List<T>(batchSize);
            var sourceList = sourceCollection.ToList();
            for (int i = 0; i < sourceList.Count; i++)
            {
                buffer.Add(sourceList[i]);
                if (((i + 1) % batchSize) == 0 && buffer.Count > 0)
                {
                    yield return buffer;
                    buffer = new List<T>(batchSize);
                }
            }

            if (buffer.Count > 0)
            {
                yield return buffer;
            }
        }
    }
}
