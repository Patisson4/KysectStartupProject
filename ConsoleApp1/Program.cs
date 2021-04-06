using System;
using System.Collections.Generic;
using System.IO;

/*
save.txt:

[<meta> MetaTaskName]
	<task status=unsolved> 
		<info> TaskName </info>
		[<subtask status=done> SubTaskName </subtask>]
	</task>
[</meta>]
*/

namespace ConsoleApp1
{
    internal class SubTask
    {
        public string TaskInfo { get; }
        public uint Id { get; }
        public bool IsCompleted { get; private set; }

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
            return "SubTask id: " + Id + "; Info: " + TaskInfo + "; Status: " + (IsCompleted ? "done" : "unsolved");
        }
    }

    internal class Task : SubTask
    {
        private TaskManagerBase Subs { get; }
        private uint AmountSubTasksDone { get; set; }  // = 0?
        internal DateTime Deadline { get; } //should be made private?? i thought it cuz DataTime is immutable 

        public Task(uint currientId, string info, string date) : base(currientId, info)
        {
            Subs = new TaskManagerBase();
            DateTime.TryParse(date, out var time);  //check for valid conversion needed
            Deadline = time;
            AmountSubTasksDone = 0;
        }
        
        public void add_sub(string info)
        {
            Subs.Add(info);
        }

        public void complete_sub(uint completeSubtaskId)
        {
            Subs.Complete(completeSubtaskId);
            AmountSubTasksDone++;
        }

        public void delete_sub(uint removeSubTaskId)
        {
            Subs.Remove(removeSubTaskId);
            AmountSubTasksDone--;
        }

        public override string ToString()
        {
            var buffer = "Task id: " + Id + "; Info: " + TaskInfo + "; Status: " +
                         (IsCompleted ? "done" : "unsolved") + "; Deadline: " + Deadline.ToString("d");
            if (Convert.ToBoolean(Subs.Tasks.Count))
                buffer += "; Progress: " + AmountSubTasksDone + "/" + Subs.Tasks.Count;
            buffer += "\n";
            foreach (var item in Subs.Tasks.Values)
                buffer += "├─> " + item.ToString() + "\n";
            return buffer;
        }
    }

    internal class Comp : IComparer<Task>
    {
        public int Compare(Task x, Task y)
        {
            return DateTime.Compare(x.Deadline, y.Deadline);
        }
    }
    
    //can handle tasks with equal id in different containers - may be fixed?
    internal class TaskManagerBase
    {
        private uint _nextId;   // = 0?
        internal virtual SortedDictionary<uint, SubTask> Tasks { get; private set; } = new();   //should be made protected

        public void Add(string value) //rename to Insert?
        {
            //add message needed?

            //check for items with equal id 
            //Tasks.ContainsValue(value);
            
            var tsk = new SubTask(_nextId, value);
            var result = Tasks.TryAdd(_nextId, tsk);  //what to do if task with next_id already exists?
            _nextId++;
        }

        public void Complete(uint id)
        {
            var result = Tasks.TryGetValue(id, out var buf);
            if (!result)
                Console.WriteLine("Error"); //throw must be here
            else //else may be removed when throw is added
                buf.Complete();
        }

        public void Remove(uint id) //rename to Erase?
        {
            var result = Tasks.Remove(id, out var buf);
            if (!result)
                Console.WriteLine("Error"); //throw must be here
            else //else may be removed when throw is added
                Console.WriteLine(buf + " removed"); //maybe: "task_info removed"?
        }

        public void Save(string path)   //needs to be moved to TaskManager
        {
            var file = new StreamWriter(path, false);

            foreach (var item in Tasks.Values)
                file.WriteLine("<subtask status = " + (item.IsCompleted ? "done" : "unsolved") + " > " + item.TaskInfo + " </subtask>");
            
            file.Close();
        }

        public void Load(string path)   //needs to be moved to TaskManager
        {
            var file = new StreamReader(path);
            string line;
            var i = 0u;
            
            //this = new TaskManagerBase();
            _nextId = 0;
            Tasks = new SortedDictionary<uint, SubTask>();
            
            while ((line = file.ReadLine()) != null)
            {
                var statment = line.Split(' ');
                
                if (string.Equals(statment[0], "<subtask"))
                {
                    Add(statment[5]);
                    if (string.Equals(statment[3], "done"))
                        Complete(i);
                    i++;
                }
            }
            file.Close();
        }
        
        public void ShowCompleted()
        {
            var amount = 0;
            
            foreach (var item in Tasks.Values)  //!!When loaded, class Task doesnt know amount of solved tasks
                if (item.IsCompleted)
                    amount++;

            if (Convert.ToBoolean(amount))
            {
                Console.WriteLine("Completed:");
                foreach (var item in Tasks.Values)
                    Console.WriteLine(item);
            }
            else
                Console.WriteLine("No completed tasks yet");
        }
        
        public void Show() //some ostream (?) override needed
        {
            if (Convert.ToBoolean(Tasks.Count))
            {
                foreach (var item in Tasks.Values)
                    Console.WriteLine(item);
            }
            else
                Console.WriteLine("No tasks to do yet");
        }
    }
    
    class Program
    {
        public static void Main()
        {
            var l = new TaskManagerBase();
            string input;

            while ((input = Console.ReadLine()) != null && input != "") //input comparision may be redundant?
            {   //Ctrl+Z to successful stop
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
                        
                        case "/stop": //FALLTHROUGH
                        case "/quit": //FALLTHROUGH
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
