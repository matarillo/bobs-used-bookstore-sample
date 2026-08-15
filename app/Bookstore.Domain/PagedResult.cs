namespace Bookstore.Domain
{
    // ISSUE-26: what the domain has to say about a paged query, and no more.
    //
    // The read side used to hand back an IPaginatedList, which knew whether there was a next page,
    // how many pages there were in total, and — the giveaway — which page numbers a pager should
    // draw buttons for. None of that is a business concept. A slice of rows, the range it was
    // taken from, and how many rows match the filter altogether: that is the whole of it. What a
    // reader should be offered to navigate with is a question for whatever is drawing the screen.
    public sealed class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, int pageIndex, int pageSize, int totalCount)
        {
            Items = items;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public IReadOnlyList<T> Items { get; }

        // One-based, as the callers and the query string have always counted pages.
        public int PageIndex { get; }

        // The size of the range that was asked for, not the number of rows that came back: the
        // last page is usually shorter, and that difference is what a pager needs to know.
        public int PageSize { get; }

        // How many rows match the filter, ignoring the range.
        public int TotalCount { get; }

        public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector) =>
            new(Items.Select(selector).ToList(), PageIndex, PageSize, TotalCount);
    }
}
