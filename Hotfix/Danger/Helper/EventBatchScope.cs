using System;

namespace ET
{
    /// <summary>
    /// 任务事件批处理作用域：Dispose 时自动 End，避免漏调 EndBatch。
    /// 用法：using (task.TaskEventBatch()) { Trigger... }
    /// </summary>
    public struct TaskEventBatchScope : IDisposable
    {
        private TaskComponentServer self;
        private bool disposed;

        public TaskEventBatchScope(TaskComponentServer self)
        {
            this.self = self;
            this.disposed = false;
            self.BeginTaskEventBatch();
        }

        public void Dispose()
        {
            if (disposed || self == null)
            {
                return;
            }
            disposed = true;
            self.EndTaskEventBatch();
            self = null;
        }
    }

}
