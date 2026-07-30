using System.ComponentModel.DataAnnotations;

public class MinAgeAttribute : ValidationAttribute
{
    private readonly int _minAge;

    public MinAgeAttribute(int minAge)
    {
        _minAge = minAge;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult("Date of birth is required");

        var dob = (DateTime)value;
        var today = DateTime.Today;

        var age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age))
            age--;

        if (age < _minAge)
            return new ValidationResult($"You must be at least {_minAge} years old");

        return ValidationResult.Success!;
    }
}
