using e_learning_vie.Models;
using System.ComponentModel.DataAnnotations;

namespace e_learning_vie.DTOs.StudentDtos
{
	public class StudentCreateDto
	{
		[Required]
		public string IdentityCode { get; set; } = null!;

		[Required]
		[StringLength(50, ErrorMessage = "First name must be less than 50 characters.")]
		public string FirstName { get; set; } = null!;

		[Required]
		[StringLength(50, ErrorMessage = "Last name must be less than 50 characters.")]
		public string LastName { get; set; } = null!;

		[DataType(DataType.Date)]
		public DateOnly? DateOfBirth { get; set; }

		[StringLength(200, ErrorMessage = "Address must be less than 200 characters.")]
		public string? Address { get; set; }

		[Phone(ErrorMessage = "Invalid phone number format.")]
		[StringLength(20, ErrorMessage = "Phone number must be less than 20 digits.")]
		public string? Phone { get; set; }

		// Mapper thủ công sang Entity
		public Student ToStudent()
		{
			return new Student
			{
				IdentityCode = this.IdentityCode,
				FirstName = this.FirstName,
				LastName = this.LastName,
				DateOfBirth = this.DateOfBirth,
				Address = this.Address,
				Phone = this.Phone
			};
		}
	}
}
