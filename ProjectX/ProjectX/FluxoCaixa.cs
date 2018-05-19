using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace ProjectX
{
    class FluxoCaixa
    {
        public List<Int64> idProduto = new List<Int64>();
        public List<decimal> precoProduto = new List<decimal>();
        public List<Venda> vendas = new List<Venda>();
        public List<Compra> compras = new List<Compra>();

        public FluxoCaixa()
        {
            CarregaDados();
            CalculaCompras();
            CalculaVendas();
            CalculaSomatorio("venda", "venda_somatorio");
            CalculaSomatorio("compra", "compra_somatorio");
            GeraBalancoPorDia();
            GeraResultadoFinalJson();
            EnviaJsonAsync();
        }

        public void EnviaJsonAsync()
        {
            string myJson = File.ReadAllText(@"C:\Users\1749\source\repos\fluxo\resultadoFormatado.json");
            using (var client = new HttpClient())
            {
                var content = new StringContent(myJson.ToString(), Encoding.UTF8, "application/json");
                var result = client.PostAsync("https://ikd29r1hsl.execute-api.us-west-1.amazonaws.com/prod/contaazul/grade", content).Result;
                Console.WriteLine(result);
            }
        }

        public void GeraResultadoFinalJson()
        {
            StreamWriter fileSomatorio = File.CreateText(@"C:\Users\1749\source\repos\fluxo\resultadoFormatadoTeste.json");
            fileSomatorio.WriteLine("{");
            fileSomatorio.WriteLine("\t\"token\": \"b79b83cb0e73477e461fb5ed8b793bb8ae373efc\",");
            fileSomatorio.WriteLine("\t\"email\": \"felipefuerback@hotmail.com\",");
            fileSomatorio.WriteLine("\t\"answer\": [");

            DirectoryInfo infoBalanco = new DirectoryInfo(@"C:\Users\1749\source\repos\fluxo\balanco");
            List<FileInfo> myfileinfosBalanco = new List<FileInfo>();
            myfileinfosBalanco.AddRange(infoBalanco.GetFiles("*.txt"));

            int contador = 0;

            decimal valorFinal = 0;
            decimal valorArredondado = 0;
            DateTime dataAtual;
            DateTime dataAnterior = new DateTime(1900, 01, 01);
            string dataFormatada = "";
            FileInfo ultimoArquivo = myfileinfosBalanco.Last();
            string ultimoCaractere = ",";
            string ultimaLinha = "";

            foreach (var info in myfileinfosBalanco)
            {
                /*informa ultima linha do ultimo arquivo*/
                if(info == ultimoArquivo)
                {
                    var lines = File.ReadLines(info.FullName);
                    ultimaLinha = lines.LastOrDefault();
                }

                foreach (var fileBalanco in File.ReadLines(info.FullName))
                {
                    /*remove a vírgula da última linha*/
                    if(ultimaLinha == fileBalanco)
                    {
                        ultimoCaractere = "";
                    }

                    dataAtual = new DateTime(int.Parse(info.Name.Substring(0, 4)), int.Parse(info.Name.Substring(4, 2)), int.Parse(info.Name.Substring(6, 2)));
                    if (dataAnterior.Year != 1900 && dataAnterior.AddDays(1) != dataAtual)
                    {
                        while (dataAtual != dataAnterior.AddDays(1))
                        {
                            DateTime novaData = dataAnterior.AddDays(1);
                            dataFormatada = novaData.Year + "-" + novaData.Month.ToString().PadLeft(2, '0') + "-" + novaData.Day.ToString().PadLeft(2, '0');
                            fileSomatorio.WriteLine("\t\t{\"date\": \"" + dataFormatada + "\", \"value\": " + valorArredondado + "},");
                            dataAnterior = novaData;
                        }
                    }

                    dataFormatada = info.Name.Substring(0, 4) + "-" + info.Name.Substring(4, 2) + "-" + info.Name.Substring(6, 2);
                    decimal valorBalanco = decimal.Parse(fileBalanco.ToString());
                    valorArredondado = decimal.Add(valorArredondado, Math.Round(valorBalanco));
                    valorFinal = decimal.Add(valorFinal, valorBalanco);
                    fileSomatorio.WriteLine("\t\t{\"date\": \"" + dataFormatada + "\", \"value\": " + valorArredondado + "}" + ultimoCaractere);
                    dataAnterior = dataAtual;
                }

                Console.Write("GERANDO ARQUIVO JSON.. contador: {0} \n", contador++);
            }

            fileSomatorio.WriteLine("\t]");
            fileSomatorio.WriteLine("}");

            fileSomatorio.Close();
        }

        public void GeraBalancoPorDia()
        {
            /*VENDA*/
            DirectoryInfo infoVenda = new DirectoryInfo(@"C:\Users\1749\source\repos\fluxo\venda_somatorio");
            List<FileInfo> myfileinfosVenda = new List<FileInfo>();
            myfileinfosVenda.AddRange(infoVenda.GetFiles("*.txt"));

            /*COMPRA*/
            DirectoryInfo infoCompra = new DirectoryInfo(@"C:\Users\1749\source\repos\fluxo\compra_somatorio");
            List<FileInfo> myfileinfosCompra = new List<FileInfo>();
            myfileinfosCompra.AddRange(infoCompra.GetFiles("*.txt"));

            int contador = 0;

            foreach (var fileInfo in myfileinfosVenda)
            {
                decimal valorBalancoDia = 0;
                string data = fileInfo.Name.Replace("_VENDA.txt", "");

                foreach (var file in File.ReadLines(fileInfo.FullName))
                {
                    valorBalancoDia = decimal.Parse(file.ToString());
                    FileInfo compra = myfileinfosCompra.Find(x => x.Name.Contains(data));

                    if (compra != null)
                    {
                        foreach (var fileCompra in File.ReadLines(compra.FullName))
                        {
                            valorBalancoDia = decimal.Subtract(valorBalancoDia, decimal.Parse(fileCompra.ToString()));
                        }
                    }
                }

                StreamWriter fileSomatorio = File.CreateText(@"C:\Users\1749\source\repos\fluxo\balanco\" + data + "_BALANCO.txt");
                fileSomatorio.WriteLine(valorBalancoDia);
                fileSomatorio.Close();

                Console.Write("GERANDO BALANCO VENDAS.. contador: {0} \n", contador++);
            }

            /*BALANCO*/
            DirectoryInfo infoBalanco = new DirectoryInfo(@"C:\Users\1749\source\repos\fluxo\balanco");
            List<FileInfo> myfileinfosBalanco = new List<FileInfo>();
            myfileinfosBalanco.AddRange(infoBalanco.GetFiles("*.txt"));

            contador = 0;

            /* Verificando se houve algum dia que teve uma compra mas não houve uma venda (Prejuízo)*/
            foreach (var fileCompra in myfileinfosCompra)
            {
                decimal valorBalancoDia = 0;
                string data = fileCompra.Name.Replace("_COMPRA.txt", "");
                FileInfo compra = myfileinfosBalanco.Find(x => x.Name.Contains(data));
                if(compra == null)
                {
                    foreach (var file in File.ReadLines(fileCompra.FullName))
                    {
                        valorBalancoDia = decimal.Parse(file.ToString());
                    }

                    StreamWriter fileSomatorio = File.CreateText(@"C:\Users\1749\source\repos\fluxo\balanco\" + data + "_BALANCO.txt");
                    fileSomatorio.WriteLine(-valorBalancoDia);
                    fileSomatorio.Close();
                }

                Console.Write("CALCULANDO BALANCO VENDAS.. contador: {0} \n", contador++);
            }
        }

        public void CalculaSomatorio(string nomeDiretorioPesquisa, string nomeDiretorioSomatorio)
        {
            DirectoryInfo info = new DirectoryInfo(@"C:\Users\1749\source\repos\fluxo\" + nomeDiretorioPesquisa);
            List<FileInfo> myfileinfos = new List<FileInfo>();
            myfileinfos.AddRange(info.GetFiles("*.txt"));

            int contador = 0;

            foreach (var fileInfo in myfileinfos)
            {
                decimal valorSomatorio = 0;
                foreach (var file in File.ReadLines(fileInfo.FullName))
                {
                    valorSomatorio = decimal.Add(valorSomatorio, decimal.Parse(file.ToString()));
                }

                StreamWriter fileSomatorio = File.CreateText(@"C:\Users\1749\source\repos\fluxo\" + nomeDiretorioSomatorio + "\\" + fileInfo.Name);
                fileSomatorio.WriteLine(valorSomatorio);
                fileSomatorio.Close();

                Console.Write("CALCULANDO SOMATORIO {0} .. contador: {1} \n", nomeDiretorioPesquisa, contador++);
            }
        }

        public void CalculaCompras()
        {
            string compraArquivoDia = "";
            DateTime novaData;

            foreach (var compra in compras.OrderBy(t=>t.Timestamp_purchase))
            {
                novaData = compra.Timestamp_purchase;

                if (compra.Payment_method_purchase == "credit")
                {
                    double vezes = compra.N_payments_purchase;
                    string s_vezes = vezes.ToString();
                    decimal valorParcela = compra.Price_purchase / decimal.Parse(s_vezes);
                    int indexParcela = 0;

                    if (compra.Timestamp_purchase.Day >= 5)
                    {
                        while (vezes != 0)
                        {
                            novaData = novaData.AddMonths(1);
                            compraArquivoDia = @"C:\Users\1749\source\repos\fluxo\compra\" + novaData.ToString("yyyyMM10") + "_COMPRA.txt";
                            StreamWriter file = File.AppendText(compraArquivoDia);
                            file.WriteLine(valorParcela);
                            file.Close();
                            indexParcela++;
                            vezes--;
                        }
                    }
                    else
                    {
                        while (vezes != 0)
                        {
                            compraArquivoDia = @"C:\Users\1749\source\repos\fluxo\compra\" + novaData.ToString("yyyyMM10") + "_COMPRA.txt";
                            StreamWriter file = File.AppendText(compraArquivoDia);
                            file.WriteLine(valorParcela);
                            file.Close();
                            novaData = novaData.AddMonths(1);
                            indexParcela++;
                            vezes--;
                        }
                    }
                }
                else /*debit*/
                {
                    compraArquivoDia = @"C:\Users\1749\source\repos\fluxo\compra\" + compra.Timestamp_purchase.ToString("yyyyMMdd") + "_COMPRA.txt";
                    StreamWriter file = File.AppendText(compraArquivoDia);
                    file.WriteLine(compra.Price_purchase);
                    file.Close();
                }

                Console.Write("CALCULANDO COMPRAS .. ID: {0} \n", compra.Purchase_id);
            }
        }

        public void CalculaVendas()
        {
            string vendaArquivoDia = "";
            DateTime novaData;

            foreach (var venda in vendas.OrderBy(t => t.TimestampSale))
            {
                novaData = venda.TimestampSale;

                if (venda.Payment_method_sale == "credit")
                {
                    double vezes = venda.N_payments;
                    string s_vezes = vezes.ToString();
                    decimal valorParcela = GetValorProduto(venda.Product_id) / decimal.Parse(s_vezes);
                    int indexParcela = 0;

                    if (venda.TimestampSale.Day >= 5)
                    {
                        while (vezes != 0)
                        {
                            novaData = novaData.AddMonths(1);
                            vendaArquivoDia = @"C:\Users\1749\source\repos\fluxo\venda\" + novaData.ToString("yyyyMM10") + "_VENDA.txt";
                            StreamWriter file = File.AppendText(vendaArquivoDia);
                            file.WriteLine(valorParcela);
                            file.Close();
                            indexParcela++;
                            vezes--;
                        }
                    }
                    else
                    {
                        while (vezes != 0)
                        {
                            vendaArquivoDia = @"C:\Users\1749\source\repos\fluxo\venda\" + novaData.ToString("yyyyMM10") + "_VENDA.txt";
                            StreamWriter file = File.AppendText(vendaArquivoDia);
                            file.WriteLine(valorParcela);
                            file.Close();
                            novaData = novaData.AddMonths(1);
                            indexParcela++;
                            vezes--;
                        }
                    }
                }
                else /*debit*/
                {
                    vendaArquivoDia = @"C:\Users\1749\source\repos\fluxo\venda\" + venda.TimestampSale.ToString("yyyyMMdd") + "_VENDA.txt";
                    StreamWriter file = File.AppendText(vendaArquivoDia);
                    file.WriteLine(GetValorProduto(venda.Product_id));
                    file.Close();
                }

                Console.Write("CALCULANDO VENDAS .. ID: {0} \n", venda.Product_id);
            }
        }

        public decimal GetValorProduto(Int64 id)
        {
            int index = idProduto.IndexOf(id);
            decimal valorProduto = precoProduto[index];
            return valorProduto;
        }

        public void CarregaDados()
        {
            //PRODUTOS
            using (var reader = new StreamReader(@"C:\Users\1749\source\repos\dados\catalog.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values[0].ToString().Replace("\"", "") != "id" && values[1].ToString().Replace("\"", "") != "price")
                    {
                        string teste = values[0].ToString().Replace("\"", "");
                        idProduto.Add(Int64.Parse(values[0].ToString().Replace("\"","")));
                        precoProduto.Add(decimal.Parse(values[1].ToString().Remove(0, 3).Replace(".", ",").Replace("\"", "")));

                        Console.Write("CARREGANDO DADOS PRODUTOS .. ID: {0} \n", values[0]);
                    }
                }
            }

            //VENDAS
            foreach (string line in File.ReadLines(@"C:\Users\1749\source\repos\dados\sales.jsonl"))
            {
                JObject jsonObject = JObject.Parse(line.ToString());
                double n_payments_sale = double.Parse(jsonObject.GetValue("n_payments").ToString());
                string saleId = jsonObject.GetValue("sale_id").ToString();
                DateTime timestampSale = DateTime.Parse(jsonObject.GetValue("timestamp").ToString());
                string payment_method_sale = jsonObject.GetValue("payment_method").ToString();
                Int64 product_id = Int64.Parse(jsonObject.GetValue("product_id").ToString());

                Venda venda = new Venda(n_payments_sale, saleId, timestampSale, payment_method_sale, product_id);
                vendas.Add(venda);

                Console.Write("CARREGANDO DADOS VENDAS .. ID: {0} \n", product_id);
            }

            //COMPRAS
            foreach (string line in File.ReadLines(@"C:\Users\1749\source\repos\dados\purchases.jsonl"))
            {
                JObject jsonObject = JObject.Parse(line.ToString());
                double n_payments_purchase = double.Parse(jsonObject.GetValue("n_payments").ToString());
                DateTime timestamp_purchase = DateTime.Parse(jsonObject.GetValue("timestamp").ToString());
                decimal price_purchase = decimal.Parse(jsonObject.GetValue("price").ToString().Remove(0, 3).Replace(".", ","));
                string purchase_id = jsonObject.GetValue("purchase_id").ToString();
                string payment_method_purchase = jsonObject.GetValue("payment_method").ToString();

                Compra compra = new Compra(n_payments_purchase, timestamp_purchase, price_purchase, purchase_id, payment_method_purchase);
                compras.Add(compra);

                Console.Write("CARREGANDO DADOS COMPRAS .. ID: {0} \n", purchase_id);
            }
        }
    }

}
