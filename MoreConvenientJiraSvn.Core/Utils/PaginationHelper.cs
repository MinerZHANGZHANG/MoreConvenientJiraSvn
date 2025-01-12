namespace MoreConvenientJiraSvn.Core.Utils;

public class PaginationHelper<T>(IEnumerable<T> data, int itemsPerPage) where T : class
{
    private readonly IEnumerable<T> _data = data;
    private readonly int _pageSize = itemsPerPage;

    public IEnumerable<T> GetCurrentItems() => _data.Skip((_currentPage - 1) * _pageSize).Take(_pageSize);

    private int _currentPage = 1;
    public int CurrentPageIndex
    {
        get { return _currentPage; }
        set
        {
            if (value > 0 && value <= GetPagesCount())
                _currentPage = value;
        }
    }

    public int GetPagesCount() => (_data.Count() - 1) / _pageSize + 1;
}
