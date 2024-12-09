using FluentValidation;
using MediatR;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Commands.Responses;
using Questao5.Application.Queries.Requests;
using Questao5.Application.Queries.Responses;
using Questao5.Domain.Entities;
using Questao5.Domain.Interfaces.Repository;
using Questao5.Domain.Language;

namespace Questao5.Application.Handlers
{
    public class SaldoContaCorrenteQueryHandler : IRequestHandler<GetContaCorrenteByNumeroQuery, SaldoContaCorrenteResponse>
    {
        private readonly IContaCorrenteRepository _contaCorrenteRepository;
        private readonly IMovimentoRepository _movimentacaoRepository;
        private readonly IValidator<GetContaCorrenteByNumeroQuery> _validator;

        public SaldoContaCorrenteQueryHandler(IContaCorrenteRepository contaCorrenteRepository, IMovimentoRepository movimentacaoRepository, IValidator<GetContaCorrenteByNumeroQuery> validator)
        {
            _contaCorrenteRepository = contaCorrenteRepository;
            _movimentacaoRepository = movimentacaoRepository;
            _validator = validator;
        }

        public async Task<SaldoContaCorrenteResponse> Handle(GetContaCorrenteByNumeroQuery request, CancellationToken cancellationToken)
        {
            var resultadoValidacao = await _validator.ValidateAsync(request);

            if (!resultadoValidacao.IsValid)
                throw new Domain.Exception.ValidationException(resultadoValidacao.Errors);

            var contaCorrente = await _contaCorrenteRepository.GetByNumero(request.Numero);

            ValidateIfContaCorrenteHasErrors(contaCorrente);

            var listaMovimentacaoCredito = await _movimentacaoRepository.GetMovimentacaoPorTipo(contaCorrente.IdContaCorrente, "C");
            var listaMovimentacaoDebito = await _movimentacaoRepository.GetMovimentacaoPorTipo(contaCorrente.IdContaCorrente, "D");

            if (listaMovimentacaoCredito.Any() is false && listaMovimentacaoDebito.Any() is false)
                return new SaldoContaCorrenteResponse(contaCorrente.Nome, contaCorrente.Numero, 0);

            var saldoContaCorrente = listaMovimentacaoCredito.Sum(c => c.Valor) - listaMovimentacaoDebito.Sum(d => d.Valor);

            return new SaldoContaCorrenteResponse(contaCorrente.Nome, contaCorrente.Numero, saldoContaCorrente);
        }


        private void ValidateIfContaCorrenteHasErrors(ContaCorrente? contaCorrente)
        {
            if (contaCorrente is null)
                throw new Domain.Exception.ValidationException(nameof(Resource.INVALID_ACCOUNT), Resource.INVALID_ACCOUNT);

            if (contaCorrente.Ativo is false)
                throw new Domain.Exception.ValidationException(nameof(Resource.INACTIVE_ACCOUNT), Resource.INACTIVE_ACCOUNT);
        }
    }
}
