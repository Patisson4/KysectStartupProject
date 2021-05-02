using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace StartingTaskManagerProject
{
    public class Manager<T> where T : BaseTask
    {
        public uint NextId { get; private set; } //public for json
        public uint CountCompleted { get; private set; } //public for json

        public SortedDictionary<uint, T> Tasks { get; private set; } //public for json
        [JsonIgnore] public ReadOnlyDictionary<uint, T> Data => new(Tasks); //for upper-level task usages

        public Manager(uint nextId = 0, uint countCompleted = 0, SortedDictionary<uint, T> tasks = null)
        {
            NextId = nextId;
            CountCompleted = countCompleted;
            Tasks = tasks ?? new SortedDictionary<uint, T>();
        }

        public bool Add(T obj)
        {
            if (Tasks.ContainsValue(obj))
                return false;

            Tasks[NextId] = obj;
            NextId++;

            return true;
        }

        public bool Complete(uint id)
        {
            if (!Tasks.TryGetValue(id, out var buf))
                return false;

            if (buf.IsCompleted) //what to do if task already completed?
                return true;

            buf.Complete();
            CountCompleted++;

            return true;
        }

        public bool Remove(uint id)
        {
            if (!Tasks.Remove(id, out var buf))
                return false;

            if (buf.IsCompleted)
                CountCompleted--;

            return true;
        }

        public void Save(string path)
        {
            var file = new StreamWriter(path, false);

            //file.Write(Compose());

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            file.Write(JsonSerializer.Serialize(this, options));

            file.Close();
        }

        public void Load(string path)
        {
            var file = new StreamReader(path);
            var obj = JsonSerializer.Deserialize<Manager<T>>(file.ReadToEnd());

            //this = obj
            NextId = obj.NextId;
            CountCompleted = obj.CountCompleted;
            Tasks = new SortedDictionary<uint, T>(obj.Tasks);

            file.Close();
        }

        public void Show()
        {
            if (Tasks.Count == 0)
                Console.WriteLine("No " + typeof(T).Name + " to do yet");
            else
                foreach (var item in Tasks)
                    Console.WriteLine("ID: " + item.Key + "; " + item.Value);
        }

        public void ShowCompleted()
        {
            if (CountCompleted == 0)
                Console.WriteLine("No completed " + typeof(T).Name + " yet");
            else
                foreach (var item in Tasks.Where(item => item.Value.IsCompleted))
                    Console.WriteLine("ID: " + item.Key + "; " + item.Value);
        }
    }
}