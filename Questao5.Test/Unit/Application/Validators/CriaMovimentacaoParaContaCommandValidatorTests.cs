using FluentAssertions;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Commands.Validadors;
using Questao5.Domain.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questao5.Test.Unit.Application.Validators
{
    public class CriaMovimentacaoParaContaCommandValidatorTests
    {
        private readonly CriaMovimentacaoParaContaCommandValidator _validator;

        public CriaMovimentacaoParaContaCommandValidatorTests()
        {
            _validator = new CriaMovimentacaoParaContaCommandValidator();
        }

        [Fact]
        public async Task Should_fail_validation_when_RequestId_is_empty()
        {
            // Arrange
            var command = new CriaMovimentacaoParaContaCommand { RequestId = "", ContaCorrenteId = "123", Valor = 100, TipoMovimentacao = "C" };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == "RequestId" && error.ErrorMessage.Contains(string.Format(Resource.PropriedadeVaziaOuNula, "RequestId")));
        }

        [Fact]
        public async Task Should_fail_validation_when_ContaCorrenteId_is_empty()
        {
            // Arrange
            var command = new CriaMovimentacaoParaContaCommand { RequestId = "RequestId", ContaCorrenteId = "", Valor = 100, TipoMovimentacao = "C" };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == "ContaCorrenteId" && error.ErrorMessage.Contains(string.Format(Resource.PropriedadeVaziaOuNula, "ContaCorrenteId")));
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Should_fail_validation_when_Valor_is_zero_or_negative(int valor)
        {
            // Arrange
            var command = new CriaMovimentacaoParaContaCommand { RequestId = "RequestId", ContaCorrenteId = "123", Valor = valor, TipoMovimentacao = "C" };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == "Valor" && error.ErrorMessage.Contains(string.Format(Resource.INVALID_VALUE, "Valor")));
        }

        [Fact]
        public async Task Should_fail_validation_when_TipoMovimentacao_is_invalid()
        {
            // Arrange
            var command = new CriaMovimentacaoParaContaCommand { RequestId = "RequestId", ContaCorrenteId = "123", Valor = 100, TipoMovimentacao = "X" };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == "TipoMovimentacao" && error.ErrorMessage == Resource.INVALID_TYPE);
        }

        [Fact]
        public async Task Should_pass_validation_when_all_properties_are_valid()
        {
            // Arrange
            var command = new CriaMovimentacaoParaContaCommand { RequestId = "request-id", ContaCorrenteId = "123", Valor = 100, TipoMovimentacao = "C" };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
