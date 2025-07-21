using e_learning_vie.Models;

namespace e_learning_vie.Services.Interfaces
{
    public interface IAcademicYearService
    {
        dynamic GetAcademicYearList();
        public Task<AcademicYear?> GetCurrentAcademicYearAsync();
    }
}
