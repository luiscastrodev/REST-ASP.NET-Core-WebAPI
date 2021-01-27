using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        //validacao de notificacoes de erro

        // validacoes de modelstatte

        // validacao de operacao de negocios

        public readonly INotificador _notificador;

        public MainController(INotificador notificador)
        {
            _notificador = notificador;
        }

        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(new
                {
                    sucess = true,
                    data = result
                });
            }

            return BadRequest(new
            {
                sucess = false,
                errors = _notificador.ObterNotificacoes().Select(n => n.Mensagem)
            }) ;
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErrorModelInvalida(modelState);

            return CustomResponse();
        }


        protected void NotificarErrorModelInvalida(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);

            foreach (var erro in erros)
            {
                var errorMessage = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;

                NotificarError(errorMessage);
            }
        }

        protected void NotificarError(string mensagem)
        {
            _notificador.Handle(new Business.Notificacoes.Notificacao(mensagem));
        }
    }
}
