using e_learning_vie.DTOs.AcademicYear;

namespace e_learning_vie.Services.Interfaces
{
    public interface IAcademicYearService
    {
        dynamic GetAcademicYearList();
        dynamic GetAcademicYearById(int id);

        dynamic CreateAcademicYear(AcademicYearDTO academicYearDTO);

        dynamic UpdateAcademicYear(int id, AcademicYearDTO academicYearDTO);
    }
}
