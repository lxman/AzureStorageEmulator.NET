using PerformanceTester.Tasks;

namespace PerformanceTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("This is the AzureStorageEmulator.NET performance analyzer.");
            Console.WriteLine();
            Console.WriteLine("1. Create/Delete Queues");
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
                        CreateDeleteQueues task = new();
                        tasks.Add(Task.Run(task.Run));
                        break;

                    case 2:
                        // Add code to run test 2
                        break;

                    case 3:
                        // Add code to run test 3
                        break;

                    case 4:
                        // Add code to run test 4
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