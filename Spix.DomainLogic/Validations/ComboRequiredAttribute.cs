using Spix.xLanguage.Resources;
using System.ComponentModel.DataAnnotations;

namespace Spix.DomainLogic.Validations;

//Un combo OBLIGATORIO.
//
//[Required] no sirve para esto: la opcion neutra [Select X] llega como Guid.Empty o 0,
//y ninguno de los dos es null, asi que Required los da por buenos. El guardado sigue,
//la base no encuentra ese padre y rechaza con error 547, que ademas llega al usuario
//como un mensaje de borrado que no tiene nada que ver.
//
//Para un combo que SI puede ir vacio NO se usa este atributo: el campo se declara
//Guid? / int? y el formulario manda null (ver ComboValue.OrNull).
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ComboRequiredAttribute : ValidationAttribute
{
    public ComboRequiredAttribute()
    {
        ErrorMessageResourceName = nameof(Resource.Validation_Combo);
        ErrorMessageResourceType = typeof(Resource);
    }

    public override bool IsValid(object? value)
    {
        return value switch
        {
            null => false,
            Guid guid => guid != Guid.Empty,
            int numero => numero != 0,
            _ => true
        };
    }
}
