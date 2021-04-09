using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ConsoleApp1
{
    internal class SubTask
    {
        protected uint Id { get; }
        internal string TaskInfo { get; }
        internal bool IsCompleted { get; private set; }

        public SubTask(uint currentId, string info)
        {
            TaskInfo = info;
            Id = currentId;
            IsCompleted = false;
        }

        public void Complete()
        {
            IsCompleted = true;
        }

        public override string ToString()
        {
            return GetType().Name + " --> ID: " + Id + "; Info: " + TaskInfo + "; Status: " + (IsCompleted ? "done" : "unsolved");
        }
    }

    internal abstract class Manager
    {
        protected internal uint NextId { get; protected set; } = 0;

        protected internal uint CountCompleted { get; protected set; } = 0;
        
        public abstract void Add(string value);

        public abstract void Complete(uint id);

        public abstract void Remove(uint id);
        
        protected abstract void Parse(string rawData);

        protected abstract string Compose();

        public void Save(string path)
        {
            var file = new StreamWriter(path, false);
            file.Write(Compose());
            file.Close();
        }

        public void Load(string path)
        {
            var file = new StreamReader(path);
            NextId = 0;
            CountCompleted = 0;
            Parse(file.ReadToEnd());
            file.Close();
        }
        
        public abstract void Show();

        public abstract void ShowCompleted();
    }
    
    internal sealed class TaskManagerBase : Manager
    {
        private SortedDictionary<uint, SubTask> _tasks = new(); //there should be a way to move these to properties to abstract Manager
        internal ReadOnlyDictionary<uint, SubTask> Data => new(_tasks);

        public override void Add(string value) //rename to Insert?
        {
            //add message needed? 
            //Tasks.ContainsValue(value); ?
            
            var tsk = new SubTask(NextId, value);
            _tasks.TryAdd(NextId, tsk);  //what to do if task with next_id already exists?
            NextId++;
        }

        public override void Complete(uint id)  //what to do if task already completed?
        {
            if (!_tasks.TryGetValue(id, out var buf))
                Console.WriteLine("Error"); //throw must be here
            else //else may be removed when throw is added
            {
                if (!buf.IsCompleted)
                    CountCompleted++;
                buf.Complete();
            }
        }

        public override void Remove(uint id) //rename to Erase?
        {
            if (!_tasks.Remove(id, out var buf))
                Console.WriteLine("Error"); //throw must be here
            else //else may be removed when throw is added
            {
                Console.WriteLine("\"" + buf + "\" removed"); //maybe: "task_info removed"?
                if (buf.IsCompleted)
                    CountCompleted--;
            }
        }

        protected override string Compose()
        {
            var buffer = "";
            foreach (var item in _tasks.Values)
                buffer += "<subtask status = " + (item.IsCompleted ? "done" : "unsolved") + " > " + item.TaskInfo + " </subtask>\n";
            return buffer;
        }

        protected override void Parse(string rawData)
        {
            var data = rawData.Split('\n');
            
            //this = new TaskManagerBase();
            _tasks = new SortedDictionary<uint, SubTask>();
            
            foreach (var line in data)
            {
                var statment = line.Split(' ');
                if (string.Equals(statment[0], "<subtask"))
                {
                    Add(statment[5]);
                    if (string.Equals(statment[3], "done"))
                        Complete(NextId - 1);
                }
            }
        }
        
        public override void ShowCompleted()
        {
            if (Convert.ToBoolean(CountCompleted))
                foreach (var item in _tasks.Values)
                {
                    if (item.IsCompleted)
                        Console.WriteLine(item);
                }
            else
                Console.WriteLine("No completed tasks yet");
        }
        
        public override void Show()
        {
            if (Convert.ToBoolean(_tasks.Count))
                foreach (var item in _tasks.Values)
                    Console.WriteLine(item);
            else
                Console.WriteLine("No tasks to do yet");
        }
    }

    class Program
    {
        public static void Main()
        {
            Manager l = new TaskManagerBase();
            string input;

            //Ctrl+Z to successful stop
            while ((input = Console.ReadLine()) != null && input != "") //input comparision may be redundant?
            {
                var statment = input.Split(' ');
                var command = statment[0];
                
                if (statment.Length == 1)
                    switch (command)
                    {
                        case "/all":
                            l.Show();
                            break;

                        case "/completed":
                            l.ShowCompleted();
                            break;
                        
                        case "/stop":
                            //FALLTHROUGH
                        case "/quit":
                            //FALLTHROUGH
                        case "/exit":
                            return;
                        
                        default:
                            Console.WriteLine("Unresolved command: " + command + "; statement disregarded");
                            break;
                    }
                else
                    switch (command)
                    {
                        case "/add":
                            l.Add(statment[1]);
                            break;
                        
                        case "/delete":
                            if (!uint.TryParse(statment[1], out var removeId))
                                Console.WriteLine("Failed to parse: " + statment[1] + " into UInt32; statement disregarded");
                            else
                                l.Remove(removeId);
                            break;
                        
                        case "/save":
                            //check for valid path???
                            l.Save(statment[1]);
                            break;
                        
                        case "/load":
                            //check for valid path!!!
                            l.Load(statment[1]);
                            break;
                        
                        case "/complete":
                            if (!uint.TryParse(statment[1], out var completeId))
                                Console.WriteLine("Failed to parse: " + statment[1] + " into UInt32; statement disregarded");
                            else
                                l.Complete(completeId);
                            break;
                        
                        default:
                            Console.WriteLine("Unresolved command: " + command + "; statement disregarded");
                            break;
                    }
            }
        }
    }
}
