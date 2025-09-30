namespace Moodify.DTO
{
	public class PagedResponseDto<T>
	{
		public int TotalCount { get; set; }     // total number of items
		public int PageNumber { get; set; }     // current page
		public int PageSize { get; set; }       // items per page
		public int TotalPages { get; set; }     // total number of pages
		public IEnumerable<T> Data { get; set; } = new List<T>(); // paged data
	}
}
