using Questao5.Domain.Entities;

namespace Questao5.Domain.Interfaces.Repository
{
    public interface IMovimentoRepository
    {
        Task AddAsync(Movimento movimento);
        Task<IEnumerable<Movimento>> GetMovimentacaoPorTipo(string contaCorrenteId, string tipoMovimentacao);
    }
}
