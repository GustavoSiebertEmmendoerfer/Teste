using MediatR;
using Microsoft.AspNetCore.Mvc;
using Questao5.Application.Queries.Requests;
using Questao5.Application.Queries.Responses;

namespace Questao5.Application.Controllers
{
    [ApiController]
    [Route("api/conta-corrente")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContaCorrenteController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Busca saldo de uma conta corrente. 
        /// </summary>
        /// <param name="numeroConta">Numero da conta</param>
        /// <returns></returns>
        [HttpGet("/conta-corrente/{numeroConta:int}")]
        [ProducesResponseType(typeof(SaldoContaCorrenteResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Buscar([FromRoute] int numeroConta)
        {
            var response = await _mediator.Send( new GetContaCorrenteByNumeroQuery() { Numero = numeroConta });
            return Ok(response);
        }
    }
}
