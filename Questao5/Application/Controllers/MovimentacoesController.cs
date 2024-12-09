using MediatR;
using Microsoft.AspNetCore.Mvc;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Commands.Responses;
using Questao5.Application.Queries.Requests;
using Questao5.Application.Queries.Responses;

namespace Questao5.Application.Controllers
{
    [ApiController]
    [Route("api/movimentacoes")]
    public class MovimentacoesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MovimentacoesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Movimenta valor de uma conta corrente. 
        /// </summary>
        /// <param name="command">Dados necessários para gerar uma movimentação</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(MovimentacaoResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> MovimentarConta([FromBody] CriaMovimentacaoParaContaCommand command)
        {
            var movimentoId = await _mediator.Send(command);
            return Ok(movimentoId);
        }
    }
}
