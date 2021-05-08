using System;
using System.IO;

namespace StartingTaskManagerProject
{
    class Program
    {
        public static void Main()
        {
            var manager = new Manager<SuperTask>();
            string input;

            //Ctrl+Z to successful stop
            while ((input = Console.ReadLine()) != null && input != "") //input comparision may be redundant?
            {
                var statment = input.Split(' ');
                try
                {
                    var command = statment[0];
                    switch (command)
                    {
                        case "/all":
                            ConsolePrinter.Show(manager);
                            break;

                        case "/completed":
                            ConsolePrinter.Show(manager, "completed");
                            break;

                        case "/today":
                            ConsolePrinter.Show(manager, "today");
                            break;

                        case "/save":
                            if (statment.Length != 2)
                                throw new ArgumentException(
                                    "Method 'Save' must be invoked with exactly 1 argument");

                            //check for valid path???
                            manager.Save(statment[1]);
                            Console.WriteLine("Progress saved successfully");
                            break;

                        case "/load":
                            if (statment.Length != 2)
                                throw new ArgumentException(
                                    "Method 'Load' must be invoked with exactly 1 argument");
                            if (!File.Exists(statment[1]))
                                throw new Exception("Failed to parse: " + statment[1] + " - No such file founded");

                            manager.Load(statment[1]);
                            Console.WriteLine("Progress loaded successfully");
                            break;

                        case "/add":
                            switch (statment.Length)
                            {
                                case < 2:
                                    throw new ArgumentException(
                                        "Method 'Add' has at least 1 parameter but is invoked with 0 arguments");
                                case 2:
                                    if (!manager.Add(new SuperTask(statment[1])))
                                        throw new Exception("An error occured while adding new " + typeof(SuperTask));
                                    break;
                                default:
                                    if (!DateTime.TryParse(statment[2], out var time))
                                        throw new ArgumentException("Failed to parse: " + statment[1] + " into DateTime");
                                    if (!manager.Add(new SuperTask(statment[1], time)))
                                        throw new Exception("An error occured while adding new " + typeof(SuperTask));
                                    break;
                            }

                            Console.WriteLine("Object added successfully");
                            break;
                        
                        case "/add-subtask":
                            if (statment.Length != 3)
                                throw new ArgumentException("Method 'Add-subtask' must be invoked with exactly 2 argument");
                            
                            if (!uint.TryParse(statment[1], out var subtaskId))
                                throw new ArgumentException("Failed to parse: " + statment[1] + " into UInt32");
                            
                            if(!manager[subtaskId].Subs.Add(new Task(statment[2])))
                                throw new Exception("An error occured while adding new " + typeof(Task));
                            
                            Console.WriteLine("Object added successfully");
                            break;

                        case "/complete":
                            if (statment.Length != 2)
                                throw new ArgumentException(
                                    "Method 'Complete' has at least 1 parameter but is invoked with 0 arguments");
                            
                            if (!uint.TryParse(statment[1], out var completeId))
                                throw new ArgumentException("Failed to parse: " + statment[1] + " into UInt32");
                            
                            if (!manager.Complete(completeId))
                                throw new Exception("An error occured while marking object as complete");

                            foreach (var (key, _) in manager[completeId].Subs.Data)  //if SuperTask is completed, then all its subtask must be also completed
                            {
                                manager[completeId].Subs.Complete(key);    //check behaviour if Complete() will be changed
                            }

                            Console.WriteLine("Object completed successfully");
                            break;
                            
                        case "/complete-subtask":
                            if (!uint.TryParse(statment[1], out var currentId))
                                throw new ArgumentException("Failed to parse: " + statment[1] + " into UInt32");
                            
                            if (!uint.TryParse(statment[2], out var subId))
                                throw new ArgumentException("Failed to parse: " + statment[2] + " into UInt32");
                            
                            if (!manager[currentId].Subs.Complete(subId))
                                throw new Exception("An error occured while marking object as complete");
                            Console.WriteLine("Object completed successfully");
                            break;

                        case "/delete":
                            switch (statment.Length)
                            {
                                case < 2:
                                    throw new ArgumentException(
                                        "Method 'Complete' has at least 1 parameter but is invoked with 0 arguments");
                                case 2:
                                    if (!uint.TryParse(statment[1], out var removeId))
                                        throw new Exception("Failed to parse: " + statment[1] + " into UInt32");
                                    if (!manager.Remove(removeId))
                                        throw new Exception("An error occured while removing object");
                                    break;
                                
                                default:    //split to separate commands
                                    throw new NotImplementedException("Can't delete subtask yet"); //should the user be able to delete subtasks?
                            }

                            Console.WriteLine("Object removed successfully");
                            break;

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