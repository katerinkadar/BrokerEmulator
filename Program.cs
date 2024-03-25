using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

internal class Program
{
    private static readonly string DirectoryPath = @"C:\BrokerStorage\"; // Путь к директории хранилища
    private static ConcurrentQueue<string> RequestQueue = new ConcurrentQueue<string>(); // Очередь запросов
    private static SemaphoreSlim Semaphore = new SemaphoreSlim(1); // Семафор для ограничения доступа к обработке запросов

    static async Task Main(string[] args)
    {
        // Создаем директорию хранилища, если она не существует
        Directory.CreateDirectory(DirectoryPath);

        // Запускаем задачи обработки запросов в отдельных потоках
        var task1 = Task.Run(ProcessRequestsAsync);
        var task2 = Task.Run(ProcessRequestsAsync);

        // Ожидаем завершения задач обработки запросов
        await Task.WhenAll(task1, task2);
    }

    private static async Task ProcessRequestsAsync()
    {
        while (true)
        {
            await Semaphore.WaitAsync(); // Захват семафора для доступа к обработке запросов

            if (RequestQueue.TryDequeue(out var requestFilePath))
            {
                await ProcessRequestAsync(requestFilePath);
            }

            Semaphore.Release(); // Освобождение семафора
            await Task.Delay(100); // Пауза для ожидания новых запросов в очереди 
        }
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
            var method = requestContent.Split('\n')[0].Trim();
            var path = requestContent.Split('\n')[1].Trim();

            // Генерируем ключ для сохранения файла ответа
            var responseKey = CalculateMd5Hash($"{method}{path}");

            // Моделируем обработку запроса (здесь может быть реализация "настоящего" брокера)
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Генерируем ответ
            var responseContent = $"200 OK\nHello, {method} {path}!";

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