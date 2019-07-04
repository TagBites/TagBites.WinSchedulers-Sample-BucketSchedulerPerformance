using System;
using System.Collections.Generic;
using System.Linq;
using TagBites.WinSchedulers;
using TagBites.WinSchedulers.Descriptors;

namespace BucketSchedulerPerformance
{
    public class SchedulerDataSource : BucketSchedulerDataSource
    {
        private const int RowsCount = 1000000;
        private const int ColumnsCount = 1000000;
        private const int MinTasksCountPerBucket = 50;
        private const int MaxTasksCountPerBucket = 100;

        private readonly object[] _rows;
        private readonly object[] _columns;
        private readonly IDictionary<(object, object), BucketModel> _buckets = new Dictionary<(object, object), BucketModel>();

        public SchedulerDataSource()
        {
            _rows = GenerateRows();
            _columns = GenerateColumns();
        }


        protected override BucketSchedulerBucketDescriptor CreateBucketDescriptor()
        {
            return new BucketSchedulerBucketDescriptor(typeof(BucketModel), nameof(BucketModel.RowResource), nameof(BucketModel.ColumnResource))
            {
                CapacityMember = nameof(BucketModel.Capacity)
            };
        }
        protected override BucketSchedulerTaskDescriptor CreateTaskDescriptor()
        {
            return new BucketSchedulerTaskDescriptor(typeof(TaskModel), nameof(TaskModel.Bucket))
            {
                ConsumptionMember = nameof(TaskModel.Consumption)
            };
        }

        public override IList<object> LoadRows() => _rows;
        public override IList<object> LoadColumns() => _columns;
        public override void LoadContent(BucketSchedulerDataSourceView view)
        {
            var rows = view.Rows;
            var columns = view.Columns;

            foreach (var row in rows)
                foreach (var column in columns)
                {
                    if (!_buckets.ContainsKey((row, column)))
                        _buckets.Add((row, column), GenerateBucket(row, column));

                    var bucket = _buckets[(row, column)];
                    view.AddBucket(bucket);
                    foreach (var task in bucket.Tasks)
                        view.AddTask(task);
                }
        }

        #region Data generation

        private readonly Random _random = new Random();
        private object[] GenerateRows()
        {
            return Enumerable.Range(0, RowsCount).Select(x => $"Workplace {x + 1}").Cast<object>().ToArray();
        }
        private object[] GenerateColumns()
        {
            return Enumerable.Range(0, ColumnsCount).Select(x => $"Day {x + 1}").Cast<object>().ToArray();
        }
        private BucketModel GenerateBucket(object row, object column)
        {
            var bucket = new BucketModel
            {
                RowResource = row,
                ColumnResource = column,
                Capacity = _random.NextDouble() * 100
            };

            var count = _random.Next(Math.Min(MinTasksCountPerBucket, MaxTasksCountPerBucket), Math.Max(MinTasksCountPerBucket, MaxTasksCountPerBucket));
            for (var i = 0; i < count; i++)
            {
                bucket.Tasks.Add(new TaskModel
                {
                    ID = i,
                    Bucket = bucket,
                    Consumption = _random.NextDouble() * 10
                });
            }

            return bucket;
        }

        #endregion

        #region Classes

        private class BucketModel
        {
            public object RowResource { get; set; }
            public object ColumnResource { get; set; }
            public List<TaskModel> Tasks { get; } = new List<TaskModel>();
            public double Capacity { get; set; }

            public override string ToString()
            {
                return Tasks.Count > 1 ? $"{Tasks.Count} tasks" : "";
            }
        }
        private class TaskModel
        {
            public int ID { get; set; }
            public BucketModel Bucket { get; set; }
            public double Consumption { get; set; }

            public override string ToString() => $"Task {ID}";
        }

        #endregion
    }
}
