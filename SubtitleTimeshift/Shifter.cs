using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
    //A classe Shifter define um método estático assíncrono chamado Shift que será responsável por aplicar o deslocamento temporal às legendas.
    public class Shifter
    {        
        //O método Shift tem os seguintes parâmetros:
        //input: o stream de entrada contendo o arquivo de legenda original.
        //output: o stream de saída onde o arquivo ajustado será gravado.
        //timeSpan: um objeto TimeSpan que define o intervalo de tempo a ser adicionado aos tempos das legendas.
        //encoding: a codificação usada para ler e escrever o arquivo (por exemplo, UTF-8).
        //bufferSize: o tamanho do buffer para leitura e escrita(valor padrão: 1024 bytes).
        //leaveOpen: booleano que indica se os streams de entrada e saída devem ser mantidos abertos após o uso.
        async static public Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
        //O método cria um StreamReader para ler o arquivo de legenda a partir do input e um StreamWriter para escrever o arquivo ajustado no output.
        //Ambos utilizam a codificação e o tamanho do buffer fornecidos nos parâmetros.
        //O parâmetro leaveOpen indica se os streams devem ser fechados automaticamente ao final do uso.
        //Se for false, os streams serão fechados automaticamente quando sair do bloco using.
            using (var reader = new StreamReader(input, encoding, true, bufferSize, leaveOpen))
            using (var writer = new StreamWriter(output, encoding, bufferSize, leaveOpen))
            {               
                string line;
                //O método entra num laço while que lê cada linha do arquivo de legenda até encontrar o fim(linha nula).
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    //Se a linha não for vazia(line.Trim().Length > 0), verifica se a linha representa um número de índice com int.TryParse.
                    //Se a linha contiver um número, este é processado.
                    if (line.Trim().Length > 0)
                    {
                        //Se a linha não for um número, é considerada como texto normal da legenda e escrita no arquivo de saída sem modificações.
                        if (int.TryParse(line, out int number))
                        {
                            //Pula para a próxima linha
                            //Lê a linha seguinte, que contém os timestamps
                            line = await reader.ReadLineAsync();
                            //Lê o tempo de início e fim
                            //A linha é dividida nos timestamps de início e fim usando Split com o separador " --> "
                            var timeParts = line.Split(new[] { " --> " }, StringSplitOptions.None);
                            if (timeParts.Length == 2)
                            {
                                //Se forem encontrados exatamente dois timestamps, estes são convertidos para TimeSpan com TimeSpan.Parse.
                                //O deslocamento temporal(timeSpan) é adicionado aos tempos de início e fim.
                                var startTime = TimeSpan.Parse(timeParts[0]);
                                var endTime = TimeSpan.Parse(timeParts[1]);

                                //Ajusta os tempos somando os timeSpan
                                startTime += timeSpan;
                                endTime += timeSpan;
                                
                                //Escreve o índice e os timestamps ajustados no arquivo de saída
                                await writer.WriteLineAsync($"{number}");
                                await writer.WriteLineAsync($"{startTime.ToString(@"hh\:mm\:ss\.fff")} --> {endTime.ToString(@"hh\:mm\:ss\.fff")}");
                            }
                            //Os StreamReader e StreamWriter são automaticamente fechados ao sair do bloco using, a menos que leaveOpen seja definido como true.
                            else
                            {
                                //Se não for um tempo, apenas escreva a linha
                                await writer.WriteLineAsync(line);
                            }
                        }
                        else
                        {
                            //Se não for um número, apenas escreva a linha
                            await writer.WriteLineAsync(line);
                        }
                    }
                    else
                    {
                        // Se for uma linha vazia, apenas escreva a linha
                        await writer.WriteLineAsync(line);
                    }
                }
            }
        }
    }
}