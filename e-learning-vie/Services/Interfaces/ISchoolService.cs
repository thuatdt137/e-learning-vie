using e_learning_vie.DTOs.School;

namespace e_learning_vie.Services.Interfaces
{
    public interface ISchoolService
    {
        List<SchoolDTO> GetSchoolList();
        SchoolDTO GetSchoolById(int id);
        dynamic AddSchool(SchoolDTO schoolDto);
        dynamic UpdateSchool(int id, SchoolDTO schoolDto);

        dynamic GetAdmissionScore(int quotaId);
    }
}
