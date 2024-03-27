using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileBroker
{
    class Program
    {
        private static readonly string DirectoryPath = @"..\..\..\..\TestFilesAll"; // Путь к директории хранилища
        private static readonly object QueueLock = new object(); // Объект для блокировки доступа к очереди запросов
        private static BlockingCollection<string> RequestQueue = new BlockingCollection<string>(); // Очередь запросов

        static async Task Main(string[] args)
        {
            // Создаем директорию хранилища, если она не существует
            Directory.CreateDirectory(DirectoryPath);

            while (true)
            {
                if (!Directory.Exists(DirectoryPath))
                {
                    Directory.CreateDirectory(DirectoryPath); // Создаем директорию, если ее нет
                }
                // Получаем все файлы запросов в директории
                var requestFiles = Directory.GetFiles(DirectoryPath, "*.req");
               
                var requestPath = Path.Combine(DirectoryPath, "338951b7e7607b65262fb051e7804d91.req");
                if (File.Exists(requestPath)) {
                  //  return null;
                }
                // Добавляем файлы в очередь запросов
                foreach (var requestFile in requestFiles)
                {
                    lock (QueueLock)
                    {
                        RequestQueue.Add(requestFile);
                    }
                }

                // Обрабатываем запросы параллельно с помощью TPL
                await Task.Run(() => ProcessRequestsAsync());

                await Task.Delay(1000); // Пауза между итерациями
            }
        }

        private static void ProcessRequestsAsync()
        {
            var tasks = new List<Task>();

            while (!RequestQueue.IsCompleted)
            {
                string requestFilePath;
                lock (QueueLock)
                {
                    if (!RequestQueue.TryTake(out requestFilePath))
                    {
                        break; // Очередь запросов пуста, завершаем обработку
                    }
                }

                var task = Task.Run(() => ProcessRequestAsync(requestFilePath));
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ProcessRequestAsync(string requestFilePath)
        {
            try
            {
                // Читаем содержимое запроса из файла
                var requestContent = await File.ReadAllTextAsync(requestFilePath);

                // Удаляем файл запроса
                File.Delete(requestFilePath);

                // Извлекаем метод и путь из запроса
                var method = requestContent.Split('"')[3].Trim();
                var path = requestContent.Split('"')[7].Trim();

                // Генерируем ключ для сохранения файла ответа
                var responseKey = CalculateMd5Hash($"{method}{path}");

                // Моделируем обработку запроса (здесь может быть реализация "настоящего" брокера)
                await Task.Delay(TimeSpan.FromSeconds(2));

                // Генерируем ответ
                var responseContent = @"{StatusCode:'200', Body:'Привет, " + method + " " + path + "'}";

                // Сохраняем файл ответа
                var responseFilePath = Path.Combine(DirectoryPath, $"{responseKey}.resp");
                await File.WriteAllTextAsync(responseFilePath, responseContent);

                Console.WriteLine("Request processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
            }
        }

        private static string CalculateMd5Hash(string input)
        {
            using (var md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
