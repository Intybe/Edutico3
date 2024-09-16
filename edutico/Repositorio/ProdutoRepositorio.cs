﻿using edutico.Data;
using edutico.Models;
using MySql.Data.MySqlClient;

namespace edutico.Repositorio
{
    public class ProdutoRepositorio : IProdutoRepositorio
    {

        public string CadastrarProduto(Produto produto, List<IFormFile> imgs, List<string> habilidades)
        {
            // Cria variável de Conexão com o Banco de Dados
            Conexao con = new Conexao();
            MySqlConnection conexao = con.ConectarBD();

            // Vairável que recebe o comando SQL
            string sql = "Call spInsertTbProduto(@codprod, @nomeProd, @descricao, @Classificacao, @Categoria, @valorUnit, @estoque);";

            MySqlCommand cmd = new MySqlCommand(sql, conexao);

            cmd.Parameters.AddWithValue("@codProd", produto.codProd);
            cmd.Parameters.AddWithValue("@nomeProd", produto.nomeProd);
            cmd.Parameters.AddWithValue("@descricao", produto.descricao);
            cmd.Parameters.AddWithValue("@Classificacao", produto.classificacao);
            cmd.Parameters.AddWithValue("@Categoria", produto.categoria);
            cmd.Parameters.AddWithValue("@valorUnit", produto.valorUnit);
            cmd.Parameters.AddWithValue("@estoque", produto.estoque);

            // Lê os dados retornados pela procedure do BD
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            // Armazena os dados retornados do Banco de Dados
            MySqlDataReader dr;

            // Executando os comandos do mysql e passsando paa a variavel dr
            dr = cmd.ExecuteReader();

            string mensagem = null;

            if (dr.Read())
            {
                mensagem = dr.GetString(0); // Captura a primeira coluna (que é a mensagem retornada)
            }
            else
            {
                mensagem = "Erro Desconhecido";
            }
            // Fechar o DataReader antes de executar outro comando
            dr.Close();

            if (mensagem == "Produto Cadastrado com sucesso!")
            {
                foreach (var img in imgs)
                {
                    string nomeImg = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    string caminhoImg = Path.Combine("wwwroot/imgs/", nomeImg);

                    using (var stream = new FileStream(caminhoImg, FileMode.Create))
                    {
                        img.CopyTo(stream);
                    }

                    string sqlImagem = "Call spInsertTbImagem(@codProd, @nomeImg, @enderecoImg);";

                    cmd = new MySqlCommand(sqlImagem, conexao);
                    cmd.Parameters.AddWithValue("@nomeImg", nomeImg);
                    cmd.Parameters.AddWithValue("@enderecoImg", caminhoImg);
                    cmd.Parameters.AddWithValue("@codProd", produto.codProd);

                    cmd.ExecuteNonQuery();
                }
            }

            foreach (var habilidade in habilidades)
            {
                string sqlHabilidade = "Call spInsertTbHabilidade_Produto(@codProd, @habilidade);";

                cmd = new MySqlCommand(sqlHabilidade, conexao);
                cmd.Parameters.AddWithValue("@codProd", produto.codProd);
                cmd.Parameters.AddWithValue("@habilidade", habilidade.Trim());

                cmd.ExecuteNonQuery();
            }

            return mensagem;


            con.DesconectarBD();

        }

        public IEnumerable<Produto> ConsultarProdutoLancamento()
        {
            // Cria variável de Conexão com o Banco de Dados
            Conexao con = new Conexao();
            MySqlConnection conexao = con.ConectarBD();

            // Comando SQL para selecionar produtos e suas respectivas imagens
            string sql = "Select * from vwProduto Left Join tbImagem On Código = tbImagem.codProd where Status = 0 and Lançamento = 0;";

            MySqlCommand cmd = new MySqlCommand(sql, conexao);

            // Lê os dados retornados pelo comando SQL
            MySqlDataReader dr = cmd.ExecuteReader();

            // Dicionário para armazenar os produtos enquanto lemos as imagens
            Dictionary<decimal, Produto> produtoDict = new Dictionary<decimal, Produto>();

            // Irá repetir o processo para cada linha retornada
            while (dr.Read())
            {
                decimal codProd;

                if (Convert.ToDecimal(dr["Código"]) != null)
                {
                    codProd = Convert.ToDecimal(dr["Código"]);
                }
                else
                {
                    // Lê os dados retornados pelo comando SQL
                    dr = cmd.ExecuteReader();
                    codProd = Convert.ToDecimal(dr["Código"]);
                }
                // Verifica se o produto já foi adicionado ao dicionário
                if (!produtoDict.ContainsKey(codProd))
                {
                    // Se o produto ainda não foi adicionado, cria uma nova instância
                    Produto produto = new Produto()
                    {
                        codProd = codProd,
                        nomeProd = dr["Nome"].ToString(),
                        descricao = dr["Descrição"].ToString(),
                        classificacao = dr["Classificação Indicativa"].ToString(),
                        categoria = dr["Categoria"].ToString(),         
                        valorUnit = Convert.ToDecimal(dr["Valor Unitário"]),
                        estoque = Convert.ToInt32(dr["Estoque"]),
                        statusProd = Convert.ToBoolean(dr["Status"]),
                        lancamento = Convert.ToBoolean(dr["Lançamento"]),
                        imgs = new List<Imagem>() // Inicializa a lista de imagens
                    };

                    // Adiciona o produto ao dicionário
                    produtoDict.Add(codProd, produto);
                }

                // Adiciona a imagem ao produto correspondente, se existir
                if (!dr.IsDBNull(dr.GetOrdinal("nomeImg")))
                {
                    Imagem imagem = new Imagem()
                    {
                        nomeImg = dr["nomeImg"].ToString(),
                        enderecoImg = dr["enderecoImg"].ToString(),
                        codProd = codProd
                    };

                    produtoDict[codProd].imgs.Add(imagem);
                }
            }

            // Fecha o leitor e a conexão com o banco de dados
            dr.Close();
            con.DesconectarBD();

            // Retorna a lista de produtos com suas imagens
            return produtoDict.Values;
        }


        public Produto ConsultarProduto(decimal codProd)
        {
            // Cria variável de Conexão com o Banco de Dados
            Conexao con = new Conexao();
            MySqlConnection conexao = con.ConectarBD();

            // Vairável que recebe o comando SQL
            string sql = "Select * from vwProduto where Status = 0;";

            MySqlCommand cmd = new MySqlCommand(sql, conexao);

            // Lê os dados retornados pela procedure do BD
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            // Armazena os dados retornados do Banco de Dados
            MySqlDataReader dr;

            // Executando os comandos do mysql e passsando paa a variavel dr
            dr = cmd.ExecuteReader();

            // Instâncianddo um objeto da classe produto para caso não retorne nada do Banco de Dados
            Produto produto = null;

            // Irá repetir o precesso para cada linha retornada
            while (dr.Read())
            {
                // Instânciando um novo objeto produto a atribuindo valores retornados para do BD
                produto = new Produto()
                {
                    codProd = Convert.ToDecimal(dr["Código"]),
                    nomeProd = dr["Nome"].ToString(),
                    descricao = dr["Descrição"].ToString(),
                    classificacao = dr["Classificação Indicativa"].ToString(),
                    categoria = dr["Categoria"].ToString(),
                    valorUnit = Convert.ToDecimal(dr["Valor Unitário"]),
                    estoque = Convert.ToInt32(dr["Estoque"]),
                    statusProd = Convert.ToBoolean(dr["Status"]),
                    lancamento = Convert.ToBoolean(dr["Lançamento"])
                };

            }

            // Fecha o leitor e a conexão com o banco de dados
            dr.Close();
            con.DesconectarBD();

            return produto;

        }
    }
}
