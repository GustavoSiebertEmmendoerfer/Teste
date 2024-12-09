using FluentAssertions;
using FluentValidation;
using MediatR;
using Newtonsoft.Json;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Commands.Responses;
using Questao5.Application.Queries.Requests;
using Questao5.Domain.Entities;
using Questao5.Domain.Interfaces.Repository;
using Questao5.Domain.Language;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Questao5.Application.Handlers
{
    public class MovimentacaoHandler : IRequestHandler<CriaMovimentacaoParaContaCommand, MovimentacaoResponse>
    {
        private readonly IContaCorrenteRepository _contaCorrenteRepository;
        private readonly IMovimentoRepository _movimentoRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IValidator<CriaMovimentacaoParaContaCommand> _validator;
        private readonly IMediator _mediator;

        public MovimentacaoHandler(IContaCorrenteRepository contaCorrenteRepository, IMovimentoRepository movimentoRepository,
            IIdempotenciaRepository idempotenciaRepository, IValidator<CriaMovimentacaoParaContaCommand> validator, IMediator mediator)
        {
            _contaCorrenteRepository = contaCorrenteRepository;
            _movimentoRepository = movimentoRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<MovimentacaoResponse?> Handle(CriaMovimentacaoParaContaCommand request, CancellationToken cancellationToken)
        {
            var resultadoValidacao = await _validator.ValidateAsync(request, cancellationToken);

            if (!resultadoValidacao.IsValid)
                 throw new Domain.Exception.ValidationException(resultadoValidacao.Errors);

            var contaCorrente = await _contaCorrenteRepository.GetById(request.ContaCorrenteId);

            ValidateIfContaCorrenteHasErrors(contaCorrente);

            var idempotenciaResult = await _idempotenciaRepository.GetById(request.RequestId);

            if (idempotenciaResult is not null)
                return JsonConvert.DeserializeObject<MovimentacaoResponse>(idempotenciaResult.Resultado);

            var movimentacao = new Movimento(contaCorrente.IdContaCorrente, request.TipoMovimentacao, request.Valor);

            await _movimentoRepository.AddAsync(movimentacao);

            var response = new MovimentacaoResponse()
            {
                MovimentacaoId = movimentacao.IdMovimento
            };

            var idempotencia = new Idempotencia(request.RequestId, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(response));

            await _idempotenciaRepository.AddAsync(idempotencia);

            return response;
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
