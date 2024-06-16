using PerformanceTester.Tasks;

namespace PerformanceTester
{
    internal class Program
    {
        private static void Main()
        {
            Console.Clear();
            Console.WriteLine("This is the AzureStorageEmulator.NET performance analyzer.");
            Console.WriteLine();
            Console.WriteLine("1. Create/Delete Queues");
            Console.WriteLine("2. Create Queue, add 1024 messages, clear all messages then delete queue");
            Console.WriteLine();
            Console.Write("Please select test(s) to run (e.g. 1,2,3,4) or hit Enter to leave: ");
            string? input = Console.ReadLine();
            if (input is null)
            {
                return;
            }
            List<int> tests = input.Split(',').Select(int.Parse).ToList();
            List<Task> tasks = [];
            foreach (int test in tests)
            {
                switch (test)
                {
                    case 1:
                        tasks.Add(Task.Run(CreateDeleteQueues.Run));
                        break;

                    case 2:
                        tasks.Add(Task.Run(CreateQueueAddDeleteMessagesDeleteQueue.Run));
                        break;

                    case 3:
                        break;

                    case 4:
                        break;

                    default:
                        Console.WriteLine($"Invalid test number: {test}");
                        break;
                }
            }
            Task.WaitAll([.. tasks]);
        }
    }
}