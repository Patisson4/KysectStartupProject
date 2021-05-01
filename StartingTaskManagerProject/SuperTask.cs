using System;

namespace StartingTaskManagerProject
{
    public class SuperTask : Task
    {
        public DateTime Deadline { get; } //should be made private?? i thought no cuz DataTime is immutable 
        public Manager<Task> Subs { get; }

        public SuperTask(string deadline, string taskInfo, bool isCompleted = false) : base(taskInfo, isCompleted)
        {
            Subs = new Manager<Task>();
            DateTime.TryParse(deadline, out var time);  //check for valid conversion needed
            Deadline = time;
        }
    
        public override string ToString()
        {
            var buffer = base.ToString() + "; Deadline: " + Deadline.ToString("d");
            if (Convert.ToBoolean(Subs.CountCompleted))
                buffer += "; Progress: " + Subs.CountCompleted + "/" + Subs.Data.Count;
            buffer += "\n";
            foreach (var item in Subs.Data.Values)
                buffer += "├─> " + item + "\n";
            return buffer;
        }  
    
    }
}