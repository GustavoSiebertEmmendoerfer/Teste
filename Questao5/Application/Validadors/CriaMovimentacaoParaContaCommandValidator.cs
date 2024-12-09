using FluentValidation;
using Questao5.Application.Commands.Requests;
using Questao5.Domain.Language;

namespace Questao5.Application.Commands.Validadors
{
    public class CriaMovimentacaoParaContaCommandValidator : AbstractValidator<CriaMovimentacaoParaContaCommand>
    {
        public CriaMovimentacaoParaContaCommandValidator()
        {
            RuleFor(c => c.RequestId)
                .NotEmpty()
                .WithMessage(string.Format(Resource.PropriedadeVaziaOuNula, "RequestId"))
                .NotNull()
                .WithMessage(string.Format(Resource.PropriedadeVaziaOuNula, "RequestId"));

            RuleFor(c => c.ContaCorrenteId)
                .NotEmpty()
                .WithMessage(string.Format(Resource.PropriedadeVaziaOuNula, "ContaCorrenteId"))
                .NotNull()
                .WithMessage(string.Format(Resource.PropriedadeVaziaOuNula, "ContaCorrenteId"));

            RuleFor(c => c.Valor)
                .GreaterThan(0)
                .WithMessage(string.Format(Resource.INVALID_VALUE, "Valor"));

            RuleFor(c => c.TipoMovimentacao)
                .Must(tipo => tipo.Equals("C") || tipo.Equals("D"))
                .WithMessage(Resource.INVALID_TYPE);
        }
    }
}