using System;
using System.IO;

namespace StartingTaskManagerProject
{
    class Program
    {
        public static void Main()
        {
            var manager = new Manager<Task>();
            string input;

            //Ctrl+Z to successful stop
            while ((input = Console.ReadLine()) != null && input != "") //input comparision may be redundant?
            {
                var statment = input.Split(' ');
                var command = statment[0];

                try
                {
                    if (statment.Length == 1)
                        switch (command)
                        {
                            case "/all":
                                manager.Show();
                                break;

                            case "/completed":
                                manager.ShowCompleted();
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
                    else
                        switch (command)
                        {
                            case "/add":
                                if (!manager.Add(new Task(statment[1])))
                                    throw new Exception("An error occured while adding new BaseTask");
                                else
                                    Console.WriteLine("Object added successfully");
                                break;

                            case "/complete":
                                if (!uint.TryParse(statment[1], out var completeId))
                                    throw new Exception("Failed to parse: " + statment[1] + " into UInt32");
                                else if (!manager.Complete(completeId))
                                    throw new Exception("An error occured while marking object as complete");
                                else
                                    Console.WriteLine("Object completed successfully");
                                break;

                            case "/delete":
                                if (!uint.TryParse(statment[1], out var removeId))
                                    throw new Exception("Failed to parse: " + statment[1] + " into UInt32");
                                else if (!manager.Remove(removeId))
                                    throw new Exception("An error occured while removing object");
                                else
                                    Console.WriteLine("Object removed successfully");

                                break;

                            case "/save":
                                //check for valid path???
                                manager.Save(statment[1]);
                                Console.WriteLine("Progress saved successfully");
                                break;

                            case "/load":
                                //check for valid path!!!
                                if (!File.Exists(statment[1]))
                                    throw new Exception("Failed to parse: " + statment[1] + " - No such file founded");
                                else
                                {
                                    manager.Load(statment[1]);
                                    Console.WriteLine("Progress loaded successfully");
                                }

                                break;

                            default:
                                throw new Exception("Unresolved command: " + command);
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "; statement disregarded");
                }
            }
        }
    }
}