using System.Text.Json;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using sqs_ex.Models;

var sqs = new AmazonSQSClient("AKIAQBXOJY2OQB4V3P3N", "V6KXw2vf0pFw/G9RHirWro8MWkFFt5CU2is0R0gc", RegionEndpoint.EUCentral1);
var queueUrl = "https://sqs.eu-central-1.amazonaws.com/003720988317/sqsexample";

while (true)
{
    Console.Clear();
    Console.WriteLine("1 - Створити заявку\n2 - Обробити всі заявки\n3 - Кількість у черзі\n0 - Вихід");
    Console.Write("\nВибір: ");
    var choice = Console.ReadLine();

    if (choice == "0") break;

    if (choice == "1")
    {
        Console.Write("Ім'я: "); var name = Console.ReadLine();
        Console.Write("Тема: "); var topic = Console.ReadLine();
        Console.Write("Опис: "); var desc = Console.ReadLine();
        Console.Write("Пріоритет (1-Low, 2-Medium, 3-High): ");
        int.TryParse(Console.ReadLine(), out int p);

        var req = new SupportRequest
        {
            UserName = name,
            Topic = topic,
            Description = desc,
            Priority = (Priority)(p is >= 1 and <= 3 ? p : 1)
        };

        await sqs.SendMessageAsync(queueUrl, JsonSerializer.Serialize(req));
        Console.WriteLine($"Заявка {req.Id} додана!");
    }
    else if (choice == "2")
    {
        ReceiveMessageResponse response;
        do
        {
            response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 2
            });

            if (response.Messages == null || response.Messages.Count == 0)
            {
                Console.WriteLine("Черга порожня.");
                break;
            }

            foreach (var message in response.Messages)
            {
                var req = JsonSerializer.Deserialize<SupportRequest>(message.Body);
                var waitTime = DateTime.UtcNow - req.CreatedAt;

                if (req.Priority == Priority.High) Console.WriteLine(" Термінова заявка!");
                Console.WriteLine($"[{req.Priority}] ID: {req.Id} | Від: {req.UserName} | Тема: {req.Topic} | Чекала: {waitTime.Minutes}хв {waitTime.Seconds}с");

                await sqs.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
            }

        } while (response.Messages.Count != 0);
    }
    else if (choice == "3")
    {
        var attr = await sqs.GetQueueAttributesAsync(new GetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            AttributeNames = new List<string> { "ApproximateNumberOfMessages" }
        });
        Console.WriteLine($"Приблизно заявок у черзі: {attr.ApproximateNumberOfMessages}");
    }

    Console.WriteLine("\nНатисніть Enter...");
    Console.ReadLine();
}