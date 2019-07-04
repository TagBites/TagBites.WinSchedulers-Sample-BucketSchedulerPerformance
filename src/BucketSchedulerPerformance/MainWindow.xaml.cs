using System.Windows;

namespace BucketSchedulerPerformance
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }


        private void Initialize()
        {
            SC_Scheduler.DataSource = new SchedulerDataSource();
            SC_Scheduler.ColumnScroller.HeaderSize = 75;
            SC_Scheduler.RowScroller.HeaderSize = 75;
        }
    }
}
