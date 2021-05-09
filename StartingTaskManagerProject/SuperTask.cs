﻿using System;

namespace StartingTaskManagerProject
{
    public class SuperTask : Task
    {
        public DateTime? Deadline { get; } //should be made private?? i thought no cuz DataTime is immutable 
        public Manager<Task> Subs { get; }
        
        public SuperTask(string taskInfo, DateTime? deadline = null, bool isCompleted = false, Manager<Task> subs = null) : base(taskInfo, isCompleted)
        {
            Subs = subs ?? new Manager<Task>();
            Deadline = deadline;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var task = (SuperTask) obj;
            return TaskInfo == task.TaskInfo && Deadline == task.Deadline;
        }
    }
}