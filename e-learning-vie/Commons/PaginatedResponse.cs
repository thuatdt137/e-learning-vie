﻿namespace e_learning_vie.Commons
{
	public class PaginatedResponse<T>
	{
		public List<T> Items { get; set; }
		public int TotalItems { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

		public PaginatedResponse(List<T> items, int totalItems, int pageNumber, int pageSize)
		{
			Items = items;
			TotalItems = totalItems;
			PageNumber = pageNumber;
			PageSize = pageSize;
		}
	}
}
