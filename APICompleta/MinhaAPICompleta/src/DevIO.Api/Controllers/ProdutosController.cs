using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers
{
    [Route("api/produtos")]
    [ApiController]
    public class ProdutosController : MainController
    {

        private readonly IProdutoRepository _produtoRepository;
        private readonly IMapper _mapper;
        private readonly IProdutoService _produtoService;

        public ProdutosController(IMapper mapper,
                                   IProdutoRepository produtoRepository, 
                                   IProdutoService produtoService,
                                   INotificador notificador)
            : base(notificador)
        {
            _produtoRepository = produtoRepository;
            _mapper = mapper;
            _produtoService = produtoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoViewModel>>> ObterTodos()
        {
            var produtos = _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());

            return Ok(produtos);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null)
            {
                return NotFound();
            }

            return Ok(produtoViewModel);
        }

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            var produto = _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));

            return produto;
        }



        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var iamgemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;

            if (!UploadArquivo(produtoViewModel.ImagemUpload, iamgemNome))
            {
                return CustomResponse();
            }

            produtoViewModel.Imagem = iamgemNome;

            var produto = _mapper.Map<Produto>(produtoViewModel);
            await _produtoRepository.Adicionar(produto);


            return CustomResponse(produtoViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null)
            {
                return NotFound();
            }

            await _produtoRepository.Remover(id);

            return CustomResponse(produtoViewModel);

        }


        private bool UploadArquivo (string arquivo, string imgNome)
        {

            if(string.IsNullOrEmpty(arquivo))
            {
                //ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto");
                NotificarError("Forneça uma imagem para este produto");
                return false;
            }

            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgNome + ".png");

            if (System.IO.File.Exists(filepath))
            {
               // ModelState.AddModelError(string.Empty, "Já existe um arquivo com este nome!");
                NotificarError("Já existe um arquivo com este nome!");

                return false;
            }
            var imageDataByteArray = Convert.FromBase64String(arquivo);
            System.IO.File.WriteAllBytes(filepath, imageDataByteArray);

            return true;
        }

    }
}
