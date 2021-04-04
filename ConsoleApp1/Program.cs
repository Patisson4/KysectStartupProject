using System;
using System.Collections.Generic;
using System.IO;

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

<completed>
</completed>
<todo>
	[<meta> MetaTaskName]
		<task> TaskName
			[<subtask> SubTaskName </subtask>]
		</task>
	[</meta>]
</todo>
*/

namespace ConsoleApp1
{
    internal partial class TaskManagerBase {}

    internal class TaskBase
    {
        public string TaskInfo { get; }
        public uint Id { get; }

        public TaskBase(uint current_id, string info)
        {
            TaskInfo = info;
            Id = current_id;
        }

        public override string ToString()
        {
            return "Task id: " + Id + "; Info: " + TaskInfo + '.';
        }
    }
/*
    internal class Task : TaskBase
    {
        private readonly TaskManagerBase _subTasks;

        public Task(uint currient_id, string info) : base(currient_id, info)
        {
            _subTasks = new TaskManagerBase();
        }
        
        public void add_sub(string info)
        {
            _subTasks.Add(info);
        }

        public void complete_sub(uint complete_id)
        {
            _subTasks.Complete(complete_id);
        }
        
    }
*/
    //can handle tasks with equal id in different containers - may be fixed?
    internal partial class TaskManagerBase
    {
        private SortedDictionary<uint, TaskBase> Finished { get; } = new();
        private SortedDictionary<uint, TaskBase> Todo { get; } = new();
        private uint next_id;   // = 0?

        public void Add(string value) //rename to Insert?
        {
            //check for items with equal id 
            //add message needed

            //Todo.ContainsValue(value);
            
            var tsk = new TaskBase(next_id, value);
            var result = Todo.TryAdd(next_id, tsk);  //what to do if task with next_id already exists?
            next_id++;
        }

        public void Complete(uint id)
        {
            var result = Todo.Remove(id, out var buf);
            if (!result)
                Console.WriteLine("Error"); //throw must be here
            else                            //else may be removed when throw is added
                Finished.Add(buf.Id, buf);
        }

        public void Remove(uint id) //rename to Erase?
        {
            var result = Todo.Remove(id, out var buf);
            if (!result)
            {
                result = Finished.Remove(id, out buf);
                if (!result)
                    Console.WriteLine("Error"); //throw must be here
            }
            Console.WriteLine(buf + " removed"); //maybe: "task_info removed"?
        }

        public void Save(string path)
        {
            var file = new StreamWriter(path, false);
            
            file.WriteLine("<completed>");
            
            foreach (var item in Finished)
                file.WriteLine("<task> " + item.Value.TaskInfo + "\n</task>");
            
            file.WriteLine("</completed>");
            file.WriteLine("<todo>");
            
            foreach (var item in Todo)
                file.WriteLine("<task> " + item.Value.TaskInfo + "\n</task>");

            file.WriteLine("</todo>");
            file.Close();
        }

        public void Load(string path)
        {
            var file = new StreamReader(path);
            string line;
            var i = 0u;
            
            file.ReadLine();
            while ((line = file.ReadLine()) != null)
            {
                var statment = line.Split(' ');
                if (string.Equals(statment[0], "</completed>"))
                    break;
                
                Console.WriteLine(statment[0] + " <task>");
                if (string.Equals(statment[0], "<task>"))
                {
                    Add(statment[1]);
                    Complete(i);
                    i++;
                }
            }
            
            file.ReadLine();
            while ((line = file.ReadLine()) != null)
            {
                var statment = line.Split(' ');
                if (string.Equals(statment[0], "</todo>"))
                    break;
                if (string.Equals(statment[0], "<task>"))
                    Add(statment[1]);
            }
            file.Close();
        }
        
        public void ShowCompleted()
        {
            if (Convert.ToBoolean(Finished.Count))
            {
                Console.WriteLine("Completed:");
                foreach (var item in Finished)
                    Console.WriteLine(item.Value);
            }
            else
                Console.WriteLine("No completed tasks yet");
        }
        
        public void Show() //some ostream (?) override needed
        {
            if (Convert.ToBoolean(Todo.Count))
            {
                Console.WriteLine("To do:");
                foreach (var item in Todo)
                    Console.WriteLine(item.Value);
            }
            else
                Console.WriteLine("No tasks to do yet");

            ShowCompleted();
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
                //I want to reject arg cuz there is commands with more than one argument
                //var arg = statment.Length > 1 ? statment[1] : ""; // input.Split(' ')[1] crashes when no-argument command (e.g. "/all") given
                
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
