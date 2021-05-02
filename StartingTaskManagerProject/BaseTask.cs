namespace StartingTaskManagerProject
{
    
    public abstract class BaseTask
    {
        public bool IsCompleted { get; private set; }

        protected BaseTask(bool isCompleted = false)
        {
            IsCompleted = isCompleted;
        }

        public void Complete()
        {
            IsCompleted = true;
        }
    }
}