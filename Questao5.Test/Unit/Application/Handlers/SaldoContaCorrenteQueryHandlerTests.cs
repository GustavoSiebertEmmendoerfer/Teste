using FluentValidation.Results;
using FluentValidation;
using NSubstitute;
using Questao5.Application.Handlers;
using Questao5.Application.Queries.Requests;
using Questao5.Domain.Entities;
using Questao5.Domain.Interfaces.Repository;
using Questao5.Domain.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute.ReturnsExtensions;

namespace Questao5.Test.Unit.Application.Handlers
{
    public class SaldoContaCorrenteQueryHandlerTests
    {
        private readonly IValidator<GetContaCorrenteByNumeroQuery> _validator;
        private readonly IContaCorrenteRepository _contaCorrenteRepository;
        private readonly IMovimentoRepository _movimentacaoRepository;
        private readonly SaldoContaCorrenteQueryHandler _handler;

        public SaldoContaCorrenteQueryHandlerTests()
        {
            _contaCorrenteRepository = Substitute.For<IContaCorrenteRepository>();
            _movimentacaoRepository = Substitute.For<IMovimentoRepository>();
            _validator = Substitute.For<IValidator<GetContaCorrenteByNumeroQuery>>();

            _handler = new SaldoContaCorrenteQueryHandler(_contaCorrenteRepository, _movimentacaoRepository, _validator);
        }

        [Fact]
        public async Task Should_throw_exception_when_invalid_request()
        {
            // Arrange
            var request = new GetContaCorrenteByNumeroQuery { Numero = -1 };

            var validationResult = new ValidationResult()
            {
                Errors = new List<ValidationFailure>
            {
                new ValidationFailure("Numero", Resource.INVALID_TRANSACTION)
            }
            };

            _validator.ValidateAsync(request, CancellationToken.None).Returns(validationResult);

            // Act
            var exception = await Assert.ThrowsAsync<Domain.Exception.ValidationException>(() => _handler.Handle(request, CancellationToken.None));

            // Assert
            exception.Message.Should().BeEquivalentTo(string.Join(",", validationResult.Errors));
        }

        [Fact]
        public async Task Should_throw_exception_when_conta_corrente_does_not_exist()
        {
            // Arrange
            var request = new GetContaCorrenteByNumeroQuery { Numero = 12345 };

            var validationResult = new ValidationResult();

            _validator.ValidateAsync(request, CancellationToken.None).Returns(validationResult);
            _contaCorrenteRepository.GetByNumero(request.Numero).ReturnsNull();

            // Act
            var exception = await Assert.ThrowsAsync<Domain.Exception.ValidationException>(() => _handler.Handle(request, CancellationToken.None));

            // Assert
            exception.ErrorType.Should().Be(nameof(Resource.INVALID_ACCOUNT));
            exception.Message.Should().Be(Resource.INVALID_ACCOUNT);
        }

        [Fact]
        public async Task Should_throw_exception_when_conta_corrente_is_inactive()
        {
            // Arrange
            var request = new GetContaCorrenteByNumeroQuery { Numero = 12345 };

            var contaCorrente = new ContaCorrente(Guid.NewGuid().ToString());
            contaCorrente.SetAtivo(false);

            var validationResult = new ValidationResult();

            _validator.ValidateAsync(request, CancellationToken.None).Returns(validationResult);
            _contaCorrenteRepository.GetByNumero(request.Numero).Returns(contaCorrente);

            // Act
            var exception = await Assert.ThrowsAsync<Domain.Exception.ValidationException>(() => _handler.Handle(request, CancellationToken.None));

            // Assert
            exception.ErrorType.Should().Be(nameof(Resource.INACTIVE_ACCOUNT));
            exception.Message.Should().Be(Resource.INACTIVE_ACCOUNT);
        }

        [Fact]
        public async Task Should_return_zero_balance_when_there_is_no_movimentacoes()
        {
            // Arrange
            var request = new GetContaCorrenteByNumeroQuery { Numero = 12345 };

            var contaCorrente = new ContaCorrente(Guid.NewGuid().ToString(), 12345, "Test Account");
            contaCorrente.SetAtivo(true);

            var validationResult = new ValidationResult();

            _validator.ValidateAsync(request, CancellationToken.None).Returns(validationResult);
            _contaCorrenteRepository.GetByNumero(request.Numero).Returns(contaCorrente);

            _movimentacaoRepository.GetMovimentacaoPorTipo(contaCorrente.IdContaCorrente, "C").Returns(new List<Movimento>());
            _movimentacaoRepository.GetMovimentacaoPorTipo(contaCorrente.IdContaCorrente, "D").Returns(new List<Movimento>());

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Saldo.Should().Be(0);
        }

        [Fact]
        public async Task Should_return_correct_balance_when_movimentacoes_exist()
        {
            // Arrange
            var request = new GetContaCorrenteByNumeroQuery { Numero = 12345 };
            var contaCorrente = new ContaCorrente(Guid.NewGuid().ToString(), 12345, "Test Account");
            contaCorrente.SetAtivo(true);

            var validationResult = new ValidationResult();

            _validator.ValidateAsync(request, CancellationToken.None).Returns(validationResult);
            _contaCorrenteRepository.GetByNumero(request.Numero).Returns(contaCorrente);

            var creditMovimentos = new List<Movimento>
            {
            new Movimento(contaCorrente.IdContaCorrente, "C", 100),
            new Movimento(contaCorrente.IdContaCorrente, "C", 200),
            };
            var debitMovimentos = new List<Movimento>
            {
            new Movimento(contaCorrente.IdContaCorrente, "D", 50),
            };

            _movimentacaoRepository.GetMovimentacaoPorTipo(contaCorrente.IdContaCorrente, "C").Returns(creditMovimentos);
            _movimentacaoRepository.GetMovimentacaoPorTipo(contaCorrente.IdContaCorrente, "D").Returns(debitMovimentos);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Saldo.Should().Be(250);
        }
    }
}
