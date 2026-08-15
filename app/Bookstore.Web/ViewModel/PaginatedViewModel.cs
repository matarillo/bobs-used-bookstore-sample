using System;
using System.Collections.Generic;
using System.Linq;
using Bookstore.Domain;

namespace Bookstore.Web.ViewModel
{
    // The pager lives here. The domain reports which rows were asked for and how
    // many match altogether (PagedResult); how many pages that makes, whether there is one either
    // side of this one, and which numbered buttons to draw are all questions about a screen.
    public abstract class PaginatedViewModel
    {
        // How many numbered buttons the pager offers at most.
        private const int PaginationButtonCount = 5;

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int PageCount => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < PageCount;

        public IEnumerable<int> PaginationButtons => PageNumbers(PageIndex, PageCount, PaginationButtonCount);

        public Dictionary<string, string> RouteData { get; set; } = new Dictionary<string, string>();

        protected void SetPage<T>(PagedResult<T> page)
        {
            PageIndex = page.PageIndex;
            PageSize = page.PageSize;
            TotalCount = page.TotalCount;
        }

        // A run of up to `count` page numbers around the current page, growing outwards until it
        // runs out of pages on both sides. Moved here unchanged from the old PaginatedList.
        // https://jithilmt.medium.com/logic-of-building-a-pagination-ui-component-a-thought-process-f057ee2d487e
        private static IEnumerable<int> PageNumbers(int pageIndex, int pageCount, int count)
        {
            if (pageCount <= 0) return Enumerable.Empty<int>();

            var pagesCount = 1;
            var start = pageIndex;
            var end = pageIndex;

            while (pagesCount < count)
            {
                var newPagesCount = pagesCount;

                if (end + 1 <= pageCount)
                {
                    end++;
                    newPagesCount++;
                }

                if (start - 1 > 0)
                {
                    start--;
                    newPagesCount++;
                }

                if (newPagesCount == pagesCount) break;

                pagesCount = newPagesCount;
            }

            return Enumerable.Range(start, pagesCount);
        }
    }
}
