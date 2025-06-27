namespace e_learning_vie.Utils
{
    public class PagingUtil
    {
        public static readonly int DefaultPageNumber = 1;
        public static readonly int DefaultPageSize = 10;
        public static readonly int MinPageSize = 10;
        public static readonly int MaxPageSize = 100;

        public static (int pageNumber, int pageSize) GetPagingParameters(int? pageNumber, int? pageSize)
        {
            int effectivePageNumber = pageNumber ?? DefaultPageNumber;
            int effectivePageSize = pageSize ?? DefaultPageSize;
            if(effectivePageNumber < 0)
            {
                effectivePageNumber = DefaultPageNumber;
            }
            if(effectivePageSize < MinPageSize)
            {
                effectivePageSize = MinPageSize;
            }
            else if(effectivePageSize > MaxPageSize)
            {
                effectivePageSize = MaxPageSize;
            }
            return (effectivePageNumber, effectivePageSize);
        }
    }
}
