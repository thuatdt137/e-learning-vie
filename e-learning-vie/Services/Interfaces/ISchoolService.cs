using e_learning_vie.DTOs.School;

namespace e_learning_vie.Services.Interfaces
{
    public interface ISchoolService
    {
        List<SchoolDTO> GetSchoolList();
        SchoolDTO GetSchoolById(int id);
        SchoolDTO AddSchool(SchoolDTO schoolDto);
        SchoolDTO UpdateSchool(int id, SchoolDTO schoolDto);
    }
}
