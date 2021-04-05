using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*
TODO...
/save file-name.txt - сохраняет все текущие задачи в указанный файл
/load file-name.txt - загружает задачи с файла
TODO...
Возможность указать дату выполнения (дедлайн)
    Информация вывродиться в /all
    Добавляется команда /today - выводит только те задачи, которые нужно сделать сегодня
Группировка задач - возможность создавать группы задач
    /create-group group-name - создает группу для задач
    /delete-group group-name - удаляет группу с заданным именем
    /add-to-group id group-name - добавляет таску с указанным id в группу с указанным именем
    /delete-from-group id group-name - удаляет задачу c группы
    Задачи, которые находятся в группе должны при выполнении /all отображаться вложенным списком
    /completed group-name - выводит все выполненные в группе задачи
Подзадачи
    Команда /add-subtask id subtask-info - добавляет к выбранной задаче подзадачу
    Добавить поддержку выполнение подзадачи по комманде /complete id
    Для задач с подзадачами выводится информация о том, сколько подзадач выполнено в формате "3/4"
Обработка ошибок - отсутствие файлов, неправильный формат ввода
    Учесть корнер кейсы. Например, задача не может быть добавлена дважды
*/

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
        private readonly TaskManagerBase _subTasks;
        public DateTime Deadline { get; }

        public Task(uint currientId, string info, string date) : base(currientId, info)
        {
            _subTasks = new TaskManagerBase();
            DateTime.TryParse(date, out var time);  //check for valid conversion needed
            Deadline = time;
        }
        
        public void add_sub(string info)
        {
            _subTasks.Add(info);
        }

        public void complete_sub(uint completeSubtaskId)
        {
            _subTasks.Complete(completeSubtaskId);
        }

        public override string ToString()
        {
            var buffer = base.ToString();
            foreach (var item in _subTasks.Tasks.Values)
            {
                buffer += Convert.ToChar(195) + item.ToString() + "\n";
            }
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
        private uint next_id;   // = 0?
        public virtual SortedDictionary<uint, SubTask> Tasks { get; } = new();

        public void Add(string value) //rename to Insert?
        {
            //add message needed?

            //check for items with equal id 
            //Tasks.ContainsValue(value);
            
            var tsk = new SubTask(next_id, value);
            var result = Tasks.TryAdd(next_id, tsk);  //what to do if task with next_id already exists?
            next_id++;
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
            Console.WriteLine(buf + " removed"); //maybe: "task_info removed"?
        }

        public void Save(string path)
        {
            var file = new StreamWriter(path, false);

            foreach (var item in Tasks.Values)
                file.WriteLine("<subtask status = " + (item.IsCompleted ? "done" : "unsolved") + " > " + item.TaskInfo + " </subtask>");
            
            file.Close();
        }

        public void Load(string path)
        {
            var file = new StreamReader(path);
            string line;
            var i = 0u;
            
            while ((line = file.ReadLine()) != null)
            {
                var statment = line.Split(' ');
                
                if (string.Equals(statment[0], "<subtask"))
                {
                    Add(statment[6]);
                    if (string.Equals(statment[4], "done"))
                        Complete(i);
                    i++;
                }
            }
            Console.WriteLine(i);
            file.Close();
        }
        
        public void ShowCompleted()
        {
            var amount = 0;
            
            foreach (var item in Tasks.Values)
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
/*
    class TaskManager : TaskManagerBase
    {
        
    }
*/    
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
                            if (!uint.TryParse(statment[1], out var remove_id))
                                Console.WriteLine("Failed to parse: " + statment[1] + " into UInt32; statement disregarded");
                            else
                                l.Remove(remove_id);
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
                            if (!uint.TryParse(statment[1], out var complete_id))
                                Console.WriteLine("Failed to parse: " + statment[1] + " into UInt32; statement disregarded");
                            else
                                l.Complete(complete_id);
                            break;
                        
                        default:
                            Console.WriteLine("Unresolved command: " + command + "; statement disregarded");
                            break;
                    }
            }
        }
    }
}
