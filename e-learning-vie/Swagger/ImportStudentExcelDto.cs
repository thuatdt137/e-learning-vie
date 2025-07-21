using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class ImportStudentExcelDto
{
    [Required(ErrorMessage = "Vui lòng chọn file Excel.")]
    public IFormFile File { get; set; }
}
