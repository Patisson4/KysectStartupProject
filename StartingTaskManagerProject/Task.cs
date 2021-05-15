namespace StartingTaskManagerProject
{
    public class Task : BaseTask
    {
        public string TaskInfo { get; }

        public Task(string taskInfo, bool isCompleted = false) : base(isCompleted)
        {
            TaskInfo = taskInfo;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var task = (Task) obj;
            return TaskInfo == task.TaskInfo;
        }
    }
}