using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StartingTaskManagerProject
{
    public class Manager<T> where T : BaseTask
    {
        public uint NextId { get; private set; }
        public uint CountCompleted { get; private set; }

        public SortedDictionary<uint, T> Tasks { get; private set; }
        [JsonIgnore] public ReadOnlyDictionary<uint, T> Data => new(Tasks); //for upper-level task usages

        public Manager(uint nextId = 0, uint countCompleted = 0, SortedDictionary<uint, T> tasks = null)
        {
            NextId = nextId;
            CountCompleted = countCompleted;
            Tasks = tasks ?? new SortedDictionary<uint, T>();
        }

        public T this[uint i]
        {
            get
            {
                if (!Tasks.ContainsKey(i))
                    throw new IndexOutOfRangeException("Index of SortedDictionary is out of range");

                return Tasks[i];
            }
        }

        public bool Add(T task)
        {
            if (Tasks.ContainsValue(task))
                return false;

            Tasks[NextId] = task;
            NextId++;

            return true;
        }

        public bool Complete(uint id)
        {
            if (!Tasks.TryGetValue(id, out var task))
                return false;

            if (task.IsCompleted)
                return true;

            task.Complete();
            CountCompleted++;

            return true;
        }

        public bool Remove(uint id)
        {
            if (!Tasks.Remove(id, out var task))
                return false;

            if (task.IsCompleted)
                CountCompleted--;

            return true;
        }

        public void Save(string path)
        {
            var file = new StreamWriter(path, false);

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
            var manager = JsonSerializer.Deserialize<Manager<T>>(file.ReadToEnd());

            NextId = manager?.NextId ?? throw new Exception("Failed to deserialize in method Load");
            CountCompleted = manager.CountCompleted;
            Tasks = new SortedDictionary<uint, T>(manager.Tasks);

            file.Close();
        }
    }
}