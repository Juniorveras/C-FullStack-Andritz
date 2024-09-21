using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
    public class Shifter
    {
        async static public Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
            using (var reader = new StreamReader(input, encoding, true, bufferSize, leaveOpen))
            using (var writer = new StreamWriter(output, encoding, bufferSize, leaveOpen))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Trim().Length > 0)
                    {
                        if (int.TryParse(line, out int number))
                        {
                            // Pula para a próxima linha
                            line = await reader.ReadLineAsync();
                            // Leia o tempo de início e fim
                            var timeParts = line.Split(new[] { " --> " }, StringSplitOptions.None);
                            if (timeParts.Length == 2)
                            {
                                var startTime = TimeSpan.Parse(timeParts[0]);
                                var endTime = TimeSpan.Parse(timeParts[1]);
                                // Ajuste os tempos
                                startTime += timeSpan;
                                endTime += timeSpan;
                                // Escreva a linha ajustada
                                await writer.WriteLineAsync($"{number}");
                                await writer.WriteLineAsync($"{startTime.ToString(@"hh\:mm\:ss\.fff")} --> {endTime.ToString(@"hh\:mm\:ss\.fff")}");
                            }
                            else
                            {
                                // Se não for um tempo, apenas escreva a linha
                                await writer.WriteLineAsync(line);
                            }
                        }
                        else
                        {
                            // Se não for um número, apenas escreva a linha
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