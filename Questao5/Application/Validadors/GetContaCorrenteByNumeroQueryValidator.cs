using FluentValidation;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Queries.Requests;
using Questao5.Domain.Language;

namespace Questao5.Application.Validadors
{
    public class GetContaCorrenteByNumeroQueryValidator : AbstractValidator<GetContaCorrenteByNumeroQuery>
    {
        public GetContaCorrenteByNumeroQueryValidator()
        {
            RuleFor(c => c.Numero)
                .NotEmpty()
                .WithMessage(string.Format(Resource.PropriedadeVaziaOuNula, "Numero"))
                .NotNull()
                .WithMessage(string.Format(Resource.PropriedadeVaziaOuNula, "Numero"))
                .GreaterThan(0)
                .WithMessage(Resource.INVALID_ACCOUNT);
        }
    }
}
