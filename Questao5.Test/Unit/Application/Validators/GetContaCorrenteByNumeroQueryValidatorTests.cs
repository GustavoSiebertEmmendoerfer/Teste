using FluentAssertions;
using Questao5.Application.Queries.Requests;
using Questao5.Application.Validadors;
using Questao5.Domain.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questao5.Test.Unit.Application.Validators
{
    public class GetContaCorrenteByNumeroQueryValidatorTests
    {
        private readonly GetContaCorrenteByNumeroQueryValidator _validator;

        public GetContaCorrenteByNumeroQueryValidatorTests()
        {
            _validator = new GetContaCorrenteByNumeroQueryValidator();
        }

        [Fact]
        public async Task Should_fail_validation_when_Numero_is_empty()
        {
            // Arrange
            var query = new GetContaCorrenteByNumeroQuery { Numero = 0 };

            // Act
            var result = await _validator.ValidateAsync(query);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == "Numero" && error.ErrorMessage.Contains(string.Format(Resource.PropriedadeVaziaOuNula, "Numero")));
        }

        [Fact]
        public async Task Should_fail_validation_when_Numero_is_negative()
        {
            // Arrange
            var query = new GetContaCorrenteByNumeroQuery { Numero = -1 };

            // Act
            var result = await _validator.ValidateAsync(query);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == "Numero" && error.ErrorMessage == Resource.INVALID_ACCOUNT);
        }

        [Fact]
        public async Task Should_pass_validation_when_Numero_is_positive()
        {
            // Arrange
            var query = new GetContaCorrenteByNumeroQuery { Numero = 123 };

            // Act
            var result = await _validator.ValidateAsync(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
