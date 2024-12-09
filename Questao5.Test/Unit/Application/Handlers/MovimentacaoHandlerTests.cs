using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Commands.Responses;
using Questao5.Application.Handlers;
using Questao5.Domain.Entities;
using Questao5.Domain.Interfaces.Repository;
using Questao5.Domain.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Questao5.Test.Unit.Application.Handlers
{
    public class MovimentacaoHandlerTests
    {
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IContaCorrenteRepository _contaCorrenteRepository;
        private readonly IMovimentoRepository _movimentoRepository;
        private readonly IValidator<CriaMovimentacaoParaContaCommand> _validator;
        private readonly MovimentacaoHandler _handler;
        private readonly IMediator _mediator;

        public MovimentacaoHandlerTests()
        {
            _contaCorrenteRepository = Substitute.For<IContaCorrenteRepository>();
            _movimentoRepository = Substitute.For<IMovimentoRepository>();
            _idempotenciaRepository = Substitute.For<IIdempotenciaRepository>();
            _validator = Substitute.For<IValidator<CriaMovimentacaoParaContaCommand>>();

            _handler = new MovimentacaoHandler(_contaCorrenteRepository, _movimentoRepository, _idempotenciaRepository, _validator, _mediator);
        }

        [Fact]
        public async Task Should_throw_exception_when_invalid_request()
        {
            // Arrange
            var request = new CriaMovimentacaoParaContaCommand { RequestId = Guid.NewGuid().ToString() };

            var validationResult = new ValidationResult()
            {
                Errors = new List<ValidationFailure>()
                {
                    new ValidationFailure("ContaCorrente", Resource.PropriedadeVaziaOuNula)
                }
            };

            _validator.ValidateAsync(request, CancellationToken.None).ReturnsForAnyArgs(validationResult);

            // Act
            var exception = await Assert.ThrowsAsync<Domain.Exception.ValidationException>(() => _handler.Handle(request, CancellationToken.None));

            // Assert
            exception.Message.Should().BeEquivalentTo(string.Join(",", validationResult.Errors));
        }

        [Fact]
        public async Task Should_throw_exception_when_invalid_conta_corrente()
        {
            // Arrange
            var request = new CriaMovimentacaoParaContaCommand { RequestId = Guid.NewGuid().ToString(), ContaCorrenteId = Guid.NewGuid().ToString() };

            var validationResult = new ValidationResult();

            _validator.ValidateAsync(request, CancellationToken.None).ReturnsForAnyArgs(validationResult);

            _contaCorrenteRepository.GetById(request.ContaCorrenteId).ReturnsNullForAnyArgs();

            // Act
            var exception = await Assert.ThrowsAsync<Domain.Exception.ValidationException>(() => _handler.Handle(request, CancellationToken.None));

            // Assert
            exception.ErrorType.Should().Be(nameof(Resource.INVALID_ACCOUNT));
            exception.Message.Should().Be(Resource.INVALID_ACCOUNT);
        }

        [Fact]
        public async Task Should_throw_exception_when_inactive_conta_corrente()
        {
            // Arrange
            var request = new CriaMovimentacaoParaContaCommand { RequestId = Guid.NewGuid().ToString(), ContaCorrenteId = Guid.NewGuid().ToString() };

            var validationResult = new ValidationResult();

            _validator.ValidateAsync(request, CancellationToken.None).ReturnsForAnyArgs(validationResult);

            var contaCorrente = new ContaCorrente(request.ContaCorrenteId);
            contaCorrente.SetAtivo(false);

            _contaCorrenteRepository.GetById(request.ContaCorrenteId).Returns(contaCorrente);

            // Act
            var exception = await Assert.ThrowsAsync<Domain.Exception.ValidationException>(() => _handler.Handle(request, CancellationToken.None));

            // Assert
            exception.ErrorType.Should().Be(nameof(Resource.INACTIVE_ACCOUNT));
            exception.Message.Should().Be(Resource.INACTIVE_ACCOUNT);
        }

        [Fact]
        public async Task Should_return_idempotencia_result_if_exists()
        {
            // Arrange
            var request = new CriaMovimentacaoParaContaCommand { RequestId = Guid.NewGuid().ToString(), ContaCorrenteId = Guid.NewGuid().ToString() };

            var validationResult = new ValidationResult();

            _validator.ValidateAsync(request, CancellationToken.None).ReturnsForAnyArgs(validationResult);

            var contaCorrente = new ContaCorrente(request.ContaCorrenteId);
            contaCorrente.SetAtivo(true);

            _contaCorrenteRepository.GetById(request.ContaCorrenteId).Returns(contaCorrente);

            var movimentacaoId = Guid.NewGuid().ToString();

            var idempotenciaResult = string.Format(@"{{MovimentacaoId: ""{0}"" }}", movimentacaoId);

            _idempotenciaRepository.GetById(request.RequestId).Returns(new Idempotencia(request.RequestId, "", idempotenciaResult));

            var expectedResult = new MovimentacaoResponse() { MovimentacaoId = movimentacaoId };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task Should_create_movimentacao_and_idempotencia()
        {
            // Arrange
            var request = new CriaMovimentacaoParaContaCommand { RequestId = Guid.NewGuid().ToString(), ContaCorrenteId = Guid.NewGuid().ToString(), Valor = 100 };

            var validationResult = new ValidationResult();

            _validator.ValidateAsync(request, CancellationToken.None).ReturnsForAnyArgs(validationResult);

            var contaCorrente = new ContaCorrente(request.ContaCorrenteId);
            contaCorrente.SetAtivo(true);

            _contaCorrenteRepository.GetById(request.ContaCorrenteId).Returns(contaCorrente);

            _idempotenciaRepository.GetById(request.RequestId).ReturnsNull();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            await _movimentoRepository.Received(1).AddAsync(Arg.Is<Movimento>(x => x.Valor == 100 &&
                                                            x.IdContaCorrente == request.ContaCorrenteId));

            await _idempotenciaRepository.Received(1).AddAsync(Arg.Is<Idempotencia>(x => x.Chave_Idempotencia == request.RequestId));
        }
    }
}
