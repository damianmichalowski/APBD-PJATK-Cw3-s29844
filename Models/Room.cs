using System.ComponentModel.DataAnnotations;
namespace APBD_PJATK_Cw3_s29844.Models;

public class Room
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nazwa sali jest wymagana.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nazwa sali musi mieć od 2 do 100 znaków.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kod budynku jest wymagany.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Kod budynku musi mieć od 1 do 10 znaków.")]
    public string BuildingCode { get; set; } = string.Empty;

    public int Floor { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Pojemność sali musi być większa od zera.")]
    public int Capacity { get; set; }

    public bool HasProjector { get; set; }

    public bool IsActive { get; set; }
}