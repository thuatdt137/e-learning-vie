namespace e_learning_vie.DTOs.StudentDtos
{
	public class StudentListDto
	{
		public int StudentId { get; set; }

		public string IdentityCode { get; set; } = null!;

		public string FirstName { get; set; } = null!;

		public string LastName { get; set; } = null!;

		public int? ClassId { get; set; }

		public int? SchoolId { get; set; }
	}
}
