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


        // 透過 ManualResetEvent 來手動封鎖或是釋放執行緖。
        // 它在被建立起來的時候會處已受信 (signaled) 狀態。
        private ManualResetEvent _manualReset = new ManualResetEvent(true);

        private int _totalCount = 999;

        public MainWindow()
        {
            this.SourceInitialized += new System.EventHandler(MainWindow_SourceInitialized);

            _myWorker.DoWork += _myWorker_DoWork;
            _myWorker.ProgressChanged += _myWorker_ProgressChanged;
            _myWorker.RunWorkerCompleted += _myWorker_RunWorkerCompleted;
        }

        private void _myWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < _totalCount; ++i)
            {
                if (_myWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }


                //當 ManualResetEvent 的狀態為未受信 (unsignaled) 狀態時會阻塞執行緒。
                _manualReset.WaitOne();

                Thread.Sleep(1);

                _myWorker.ReportProgress(i + 1);

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
                //將 ManualResetEvent 的狀態切換為已受信 (signaled) 狀態，讓它在呼叫 WaitOne() 方法時讓執行緒繼續執行。
                _manualReset.Set();
            }
        }

        private void tglPauseResume_Unchecked(object sender, RoutedEventArgs e)
        {
            //將 ManualResetEvent 的狀態切換為未受信 (unsignaled) 狀態，讓它在呼叫 WaitOne() 方法時阻擋執行緒繼續執行。
            _manualReset.Reset();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _myWorker.CancelAsync();
        }
    }
}