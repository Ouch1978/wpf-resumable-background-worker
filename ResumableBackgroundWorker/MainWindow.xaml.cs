using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace ResumableBackgroundWorker
{
    public partial class MainWindow : Window
    {
        private BackgroundWorker _myWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        private ManualResetEvent _manualReset = new ManualResetEvent(true);

        private int _totalCount = 9999;

        public MainWindow()
        {
            this.SourceInitialized += new System.EventHandler(MainWindow_SourceInitialized);

            _myWorker.DoWork += _myWorker_DoWork;
            _myWorker.ProgressChanged += _myWorker_ProgressChanged;
            _myWorker.RunWorkerCompleted += _myWorker_RunWorkerCompleted;
        }

        private void _myWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < _totalCount; i++)
            {
                if (_myWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                _manualReset.WaitOne();

                Thread.Sleep(1);

                _myWorker.ReportProgress(++i);

            }
        }

        private void _myWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            txtMessage.Text = $"{e.ProgressPercentage.ToString().PadLeft(_totalCount.ToString().Length)}/{_totalCount}";
        }

        private void _myWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            btnStop.IsEnabled = false;
        }

        private void tglPauseResume_Checked(object sender, RoutedEventArgs e)
        {
            if (_myWorker.IsBusy == false)
            {
                _myWorker.RunWorkerAsync();
            }
            else
            {
                _manualReset.Set();
            }
        }

        private void tglPauseResume_Unchecked(object sender, RoutedEventArgs e)
        {
            _manualReset.Reset();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _myWorker.CancelAsync();
        }
    }
}