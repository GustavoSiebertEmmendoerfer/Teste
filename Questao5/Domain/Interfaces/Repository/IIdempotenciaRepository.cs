using Questao5.Domain.Entities;

namespace Questao5.Domain.Interfaces.Repository
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia> GetById(string IdIdempotencia);
        Task AddAsync(Idempotencia idempotencia);
    }
}
