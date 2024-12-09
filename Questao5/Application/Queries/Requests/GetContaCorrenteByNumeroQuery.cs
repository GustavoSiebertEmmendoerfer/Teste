using MediatR;
using Questao5.Application.Queries.Responses;

namespace Questao5.Application.Queries.Requests
{
    public class GetContaCorrenteByNumeroQuery : IRequest<SaldoContaCorrenteResponse>
    {
        public int Numero { get; set; }
    }
}
