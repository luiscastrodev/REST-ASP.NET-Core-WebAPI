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

        //Enviando o arquivo fisico
        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo(ProdutoImagemViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";

            if (!await UploadArquivoAlternativo(produtoViewModel.ImagemUpload, imgPrefixo))
            {
                return CustomResponse();
            }

            produtoViewModel.Imagem = imgPrefixo + produtoViewModel.ImagemUpload.FileName;

            var produto = _mapper.Map<Produto>(produtoViewModel);
            await _produtoRepository.Adicionar(produto);


            return CustomResponse(produtoViewModel);
        }



        //Arquivo base 64
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

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Atualizar(Guid id, ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id)
            {
                NotificarError("O Id Informado não é valido");
                return CustomResponse(produtoViewModel);
            }


            var produtoAtualizacao = await ObterProduto(id);
            produtoViewModel.Imagem = produtoViewModel.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if(produtoViewModel.ImagemUpload != null)
            {
                var iamgemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;

                if (!UploadArquivo(produtoViewModel.ImagemUpload, iamgemNome))
                {
                    return CustomResponse();
                }
                produtoViewModel.Imagem = iamgemNome;

            }

            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Nome = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;


            var produto = _mapper.Map<Produto>(produtoAtualizacao);
            await _produtoRepository.Atualizar(produto);


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

        //base64
        private bool UploadArquivo(string arquivo, string imgNome)
        {

            if (string.IsNullOrEmpty(arquivo))
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

        //arquivo fisico
        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {

            if (arquivo == null || arquivo.Length <= 0)
            {
                //ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto");
                NotificarError("Forneça uma imagem para este produto");
                return false;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-wrbapi/src/assets", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                // ModelState.AddModelError(string.Empty, "Já existe um arquivo com este nome!");
                NotificarError("Já existe um arquivo com este nome!");

                return false;
            }
            using (var stream = new FileStream(path, FileMode.Create)) {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }


        //teste
        [RequestSizeLimit(400000000)]
        [HttpPost("imagem")]
        public async Task<ActionResult> AdicionarImagem(IFormFile file)
        {
            return Ok();
        }


    }
}
