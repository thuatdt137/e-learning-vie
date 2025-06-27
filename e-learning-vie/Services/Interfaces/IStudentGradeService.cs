using e_learning_vie.ModelsDTO.Student;
using System.Security.Claims;

namespace e_learning_vie.Services.Interfaces
{
    public interface IStudentGradeService
    {
        Task<StudentGradeDto> GetGradesByAcademicYearAsync(ClaimsPrincipal user, int? academicYearId = null);
        Task<List<AcademicYearDto>> GetAcademicYearsWithGradesAsync(ClaimsPrincipal user);

    }
}
