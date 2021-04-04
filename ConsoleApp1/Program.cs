using System;
using System.Collections.Generic;

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
namespace ConsoleApp1
{
    internal partial class TaskManager {}

    internal class Task_base
    {
        public string TaskInfo { get; }
        public uint Id { get; }

        public Task_base(uint current_id, string info)
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
    internal class Task : Task_base
    {
        private readonly TaskManager _subTasks;

        public Task(uint currient_id, string info) : base(currient_id, info)
        {
            _subTasks = new TaskManager();
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
    internal partial class TaskManager
    {
        private readonly SortedDictionary<uint, Task_base> _todo = new(), _finished = new();
        private uint next_id;   // = 0?

        public void Add(string value) //rename to Insert?
        {
            //check for items with equal id 
            //add message needed

            //_todo.ContainsValue(value);
            
            var tsk = new Task_base(next_id, value);
            var result = _todo.TryAdd(next_id, tsk);  //what to do if task with next_id already exists?
            next_id++;
        }

        public void Complete(uint id)
        {
            var result = _todo.Remove(id, out var buf);
            if (!result)
                Console.WriteLine("Error"); //throw must be here
            else                            //else may be removed when throw is added
                _finished.Add(buf.Id, buf);
        }

        public void ShowCompleted()
        {
            if (Convert.ToBoolean(_finished.Count))
            {
                Console.WriteLine("Completed:");
                foreach (var item in _finished)
                    Console.WriteLine(item.Value);
            }
            else
                Console.WriteLine("No completed tasks yet");
        }
        
        public void Show() //some ostream (?) override needed
        {
            if (Convert.ToBoolean(_todo.Count))
            {
                Console.WriteLine("To do:");
                foreach (var item in _todo)
                    Console.WriteLine(item.Value);
            }
            else
                Console.WriteLine("No tasks to do yet");

            ShowCompleted();
        }

        public void Remove(uint id) //rename to Erase?
        {
            var result = _todo.Remove(id, out var buf);
            if (!result)
            {
                result = _finished.Remove(id, out buf);
                if (!result)
                    Console.WriteLine("Error"); //throw must be here
            }
            Console.WriteLine(buf + " removed"); //maybe: "task_info removed"?
        }
    }
    
    class Program
    {
        public static void Main()
        {
            var l = new TaskManager();
            string input;

            while ((input = Console.ReadLine()) != null && input != "") //input comparision may be redundant?
            {   //Ctrl+Z to successful stop
                var statment = input.Split(' ');
                var command = statment[0];
                //I want to reject arg cuz there is commands with more than one argument
                var arg = statment.Length > 1 ? statment[1] : ""; // input.Split(' ')[1] crashes when no-argument command (e.g. "/all") given
                switch (command)
                {
                    case "/add":
                        l.Add(statment[1]);
                        break;
                    
                    case "/all":
                        l.Show();
                        break;
                    
                    case "/delete":
                        if (!uint.TryParse(statment[1], out var remove_id))
                            Console.WriteLine("Failed to parse: " + statment[1] + " into UInt32; statement disregarded");
                        else
                            l.Remove(remove_id);
                        break;
                    
                    case "/save":
                        //TODO...
                        break;
                    
                    case "/load":
                        //TODO...
                        break;
                    
                    case "/complete":
                        if (!uint.TryParse(statment[1], out var complete_id))
                            Console.WriteLine("Failed to parse: " + statment[1] + " into UInt32; statement disregarded");
                        l.Complete(complete_id);
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
            }
        }
    }
}
