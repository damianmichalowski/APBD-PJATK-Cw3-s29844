using System.ComponentModel.DataAnnotations;
namespace APBD_PJATK_Cw3_s29844.Models;

public class Reservation : IValidatableObject
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Id sali musi być większe od zera.")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Nazwa organizatora jest wymagana.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nazwa organizatora musi mieć od 2 do 100 znaków.")]
    public string OrganizerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Temat rezerwacji jest wymagany.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Temat musi mieć od 3 do 200 znaków.")]
    public string Topic { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [Required(ErrorMessage = "Status jest wymagany.")]
    [RegularExpression("^(planned|confirmed|cancelled)$",
        ErrorMessage = "Status może mieć tylko wartość: planned, confirmed albo cancelled.")]
    public string Status { get; set; } = "planned";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime <= StartTime)
        {
            yield return new ValidationResult(
                "EndTime musi być późniejsze niż StartTime.",
                new[] { nameof(StartTime), nameof(EndTime) }
            );
        }
    }
}