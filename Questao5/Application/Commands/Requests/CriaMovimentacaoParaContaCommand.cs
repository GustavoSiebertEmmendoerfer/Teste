using MediatR;
using Questao5.Application.Commands.Responses;

namespace Questao5.Application.Commands.Requests
{
    public class CriaMovimentacaoParaContaCommand : IRequest<MovimentacaoResponse>
    {
        public string RequestId { get; set; }
        public string ContaCorrenteId { get; set; }
        public decimal Valor { get; set; }
        public string TipoMovimentacao { get; set; }
    }
}
