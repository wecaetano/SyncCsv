using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DownloadFile
{
    class Program
    {
        static void Main(string[] args)
        {
            // Nome do arquivo JSON de configuração
            string configFileName = "config.json";

            // Valores padrão para as configurações
            string fileUrl = "http://13.90.135.11:3000/projects/projetos-telecom/issues.csv?query_id=4";
            string username = "powerbi";
            string password = "powerbi@2023";
            string localFolder = "C:\\Downloads";

            // Verifica se o arquivo JSON de configuração existe
            if (File.Exists(configFileName))
            {
                // Lê as configurações do arquivo JSON
                JObject config = JObject.Parse(File.ReadAllText(configFileName));
                fileUrl = (string)config["fileUrl"];
                username = (string)config["username"];
                password = (string)config["password"];
                localFolder = (string)config["localFolder"];
            }
            else
            {
                // Cria o arquivo JSON de configuração com os valores padrão
                JObject config = new JObject
                {
                    ["fileUrl"] = fileUrl,
                    ["username"] = username,
                    ["password"] = password,
                    ["localFolder"] = localFolder
                };
                File.WriteAllText(configFileName, config.ToString());
            }

            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

            RestClient client = new RestClient(fileUrl);
            RestRequest request = new RestRequest(fileUrl, Method.Get);
            request.AddParameter("Content-Type", "text/csv; header=present", ParameterType.HttpHeader);
            request.AddParameter("Authorization", $"Basic {authString}", ParameterType.HttpHeader);
            RestResponse response = client.Execute(request);

            using (Stream stream = new MemoryStream(response.RawBytes!))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                // Crie um nome de arquivo a partir da URL do arquivo
                string fileName = new Uri(fileUrl).Segments[^1];

                // Crie um caminho de arquivo na pasta local
                string filePath = Path.Combine(localFolder, fileName);

                // Verifica se a pasta local existe, caso contrário, cria a pasta
                if (!Directory.Exists(localFolder))
                {
                    Directory.CreateDirectory(localFolder);
                }

                // Salve o arquivo na pasta local
                File.WriteAllBytes(filePath, fileBytes);
            }
        }
    }
}