using System;
using System.IO;

namespace StartingTaskManagerProject
{
    internal static class Program
    {
        public static void Main()
        {
            var taskManager = new Manager<SuperTask>();
            string input;

            while ((input = Console.ReadLine()) != null && input != "") //input comparision may be redundant?
            {
                var statement = input.Split(' ');
                try
                {
                    var command = statement[0];
                    switch (command)
                    {
                        case "/all":
                            ConsolePrinter.Show(taskManager);
                            break;

                        case "/completed":
                            ConsolePrinter.Show(taskManager, "completed");
                            break;

                        case "/today":
                            ConsolePrinter.Show(taskManager, "today");
                            break;

                        case "/save":
                            if (statement.Length != 2)
                                throw new ArgumentException(
                                    "Method 'Save' must be invoked with exactly 1 argument");

                            if (!Directory.Exists(statement[1]))
                                throw new ArgumentException("No such directory founded");

                            taskManager.Save(statement[1]);

                            Console.WriteLine("Progress saved successfully");
                            break;

                        case "/load":
                            if (statement.Length != 2)
                                throw new ArgumentException(
                                    "Method 'Load' must be invoked with exactly 1 argument");

                            if (!File.Exists(statement[1]))
                                throw new Exception("No such file founded");

                            taskManager.Load(statement[1]);

                            Console.WriteLine("Progress loaded successfully");
                            break;

                        case "/add":
                            switch (statement.Length)
                            {
                                case 2:
                                    if (!taskManager.Add(new SuperTask(statement[1])))
                                        throw new Exception("An error occured while adding new " + typeof(SuperTask));
                                    break;

                                case 3:
                                    if (!DateTime.TryParse(statement[2], out var time))
                                        throw new ArgumentException("Failed to parse: " + statement[1] +
                                                                    " into DateTime");

                                    if (!taskManager.Add(new SuperTask(statement[1], time)))
                                        throw new Exception("An error occured while adding new " + typeof(SuperTask));
                                    break;

                                default:
                                    throw new ArgumentException("Method 'Add' invoked with wrong amount of arguments");
                            }

                            Console.WriteLine("Object added successfully");
                            break;

                        case "/add-subtask":
                        {
                            if (statement.Length != 3)
                                throw new ArgumentException(
                                    "Method 'Add-subtask' must be invoked with exactly 2 argument");

                            if (!uint.TryParse(statement[1], out var taskId))
                                throw new ArgumentException("Failed to parse: " + statement[1] + " into UInt32");

                            if (!taskManager[taskId].Subs.Add(new Task(statement[2])))
                                throw new Exception("An error occured while adding new " + typeof(Task));

                            Console.WriteLine("Object added successfully");
                            break;
                        }

                        case "/complete":
                            if (statement.Length != 2)
                                throw new ArgumentException(
                                    "Method 'Complete' has at least 1 parameter but is invoked with 0 arguments");

                            if (!uint.TryParse(statement[1], out var completeId))
                                throw new ArgumentException("Failed to parse: " + statement[1] + " into UInt32");

                            if (!taskManager.Complete(completeId))
                                throw new Exception("An error occured while marking object as complete");

                            //if SuperTask is completed, then all its subtask must be also completed
                            foreach (var (key, _) in taskManager[completeId].Subs.Data)
                                taskManager[completeId].Subs.Complete(key);

                            //check behaviour of previous for if Complete() will be changed

                            Console.WriteLine("Object completed successfully");
                            break;

                        case "/complete-subtask":
                        {
                            if (!uint.TryParse(statement[1], out var taskId))
                                throw new ArgumentException("Failed to parse: " + statement[1] + " into UInt32");

                            if (!uint.TryParse(statement[2], out var subtaskId))
                                throw new ArgumentException("Failed to parse: " + statement[2] + " into UInt32");

                            if (!taskManager[taskId].Subs.Complete(subtaskId))
                                throw new Exception("An error occured while marking object as complete");

                            Console.WriteLine("Object completed successfully");
                            break;
                        }

                        case "/delete":
                            //although method is called 'Remove', command is called 'Delete', that's why there is such a comment
                            if (statement.Length != 2)
                                throw new ArgumentException(
                                    "Method 'Delete' has at least 1 parameter but is invoked with 0 arguments");

                            if (!uint.TryParse(statement[1], out var removeId))
                                throw new Exception("Failed to parse: " + statement[1] + " into UInt32");

                            if (!taskManager.Remove(removeId))
                                throw new Exception("An error occured while deleting object");

                            Console.WriteLine("Object deleted successfully");
                            break;

                        //TODO: if you add a subtask, complete it and delete this subtask - the parent task will be marked as completed, although it shouldn't
                        case "/delete-subtask":
                        {
                            if (!uint.TryParse(statement[1], out var taskId))
                                throw new ArgumentException("Failed to parse: " + statement[1] + " into UInt32");

                            if (!uint.TryParse(statement[2], out var subtaskId))
                                throw new ArgumentException("Failed to parse: " + statement[2] + " into UInt32");

                            if (!taskManager[taskId].Subs.Remove(subtaskId))
                                throw new Exception("An error occured while deleting subtask");

                            Console.WriteLine("Object deleted successfully");
                            break;
                        }

                        case "/stop":
                        //FALLTHROUGH
                        case "/quit":
                        //FALLTHROUGH
                        case "/exit":
                            return;

                        default:
                            throw new Exception("Unresolved command: " + command);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetType().Name + ": " + e.Message + "; statement disregarded");
                }
            }
        }
    }
}