using Questao5.Domain.Entities;

namespace Questao5.Domain.Interfaces.Repository
{
    public interface IContaCorrenteRepository
    {
        Task<ContaCorrente> GetById(string contaCorrenteId);
        Task<ContaCorrente> GetByNumero(int numero);
    }
}
