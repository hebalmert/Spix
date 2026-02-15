using System.ComponentModel.DataAnnotations;

namespace Spix.AppInfra.Validations;

public static class ValidatorModel
{
    public static bool IsValid(object model, out List<ValidationResult> results)
    {
        var context = new ValidationContext(model, null, null);
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(model, context, results, true);
    }

}