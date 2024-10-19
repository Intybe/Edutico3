﻿using edutico.Libraries.Login;
using edutico.Models;
using edutico.Repositorio;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace edutico.Controllers
{
    public class ProdutoController : Controller
    {
        private IProdutoRepositorio? _produtoRepositorio;
        private readonly LoginSessao _loginSessao;

        // Construtor com injeção de dependência
        public ProdutoController(IProdutoRepositorio produtoRepositorio, LoginSessao loginSessao)
        {
            _produtoRepositorio = produtoRepositorio;
            _loginSessao = loginSessao;
        }

        public IActionResult CadastroProduto()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CadastrarProduto(string nomeProd, string codProd, string descricaoProd, string classificacao, string habilidadesEnviadas, string valorUnit, string estoque, string categoria, string lacamentoProd, List<IFormFile> imgs)
        {
            if (imgs == null || !imgs.Any())
            {
                ViewData["msg"] = "Nenhuma imagem foi enviada!";
                return View("CadastroProduto");
            }
            else
            {
                Produto produto = new Produto()
                {
                    codProd = Convert.ToDecimal(codProd),
                    nomeProd = Convert.ToString(nomeProd),
                    descricao = Convert.ToString(descricaoProd),
                    classificacao = Convert.ToString(classificacao),
                    categoria = Convert.ToString(categoria),
                    valorUnit = Convert.ToDecimal(valorUnit),
                    estoque = Convert.ToInt32(estoque),
                    lancamento = Convert.ToBoolean(lacamentoProd)
                };

                List<string> habilidades = habilidadesEnviadas.Split(',').ToList();

                // Chamar o repositório e passar o objeto produto e as imagens
                string resultado = _produtoRepositorio.CadastrarProduto(produto, imgs, habilidades);

                ViewData["msg"] = resultado;
                return View("CadastroProduto");
            }
        }

        public IActionResult DetalhesProduto(decimal? codProd)
        {
            Produto produto = null;

            // Verifica se o codProd é nulo ou não foi passado
            if (!codProd.HasValue)
            {
                // Criar o produto padrão
                produto = new Produto()
                {
                    codProd = 1, // Código do produto
                    nomeProd = "Nome Produto Padrão",
                    descricao = "Descrição Padrão",
                    classificacao = "Classificação Indicativa",
                    categoria = "Categoria",
                    valorUnit = 0, // Valor padrão
                    estoque = 100, // Estoque padrão
                    statusProd = true, // Produto ativo
                    lancamento = false, // Não é lançamento
                    imgs = new List<Imagem>() // Inicializa a lista de imagens
                };

                // Adiciona múltiplas imagens padrão ao produto
                for (int i = 1; i <= 5; i++)
                {
                    Imagem imagem = new Imagem
                    {
                        enderecoImg = "~/imgs/img_prod_padrao_quadrada.png" // Caminhos de imagem diferentes
                    };

                    produto.imgs.Add(imagem); // Adiciona a imagem à lista de imagens do produto
                }
            }
            else
            {
                // Consultar o produto pelo código
                produto = _produtoRepositorio.ConsultarDetalheProduto(codProd.Value);
            }

            return View(produto);
        }

        public IActionResult Espelho()
        {
            return View();
        }

        public IActionResult CadastrarAvaliacao(decimal codProd, int qtdEstrela, string comentario)
        {
            // Pega o codLogin do Usuário Logado através da sessão
            var codLogin = _loginSessao.GetLogin();

            if (codLogin == null)
            {
                // Se o cliente não estiver logado, redireciona para a página de login
                return RedirectToAction("Login", "Login");
            }

            string mensagem = _produtoRepositorio.CadastrarAvaliacao(qtdEstrela, comentario, codLogin.codLogin, codProd);

            TempData["msg"] = mensagem;
            return RedirectToAction("DetalhesProduto", new { codProd });
        }
    }
}

