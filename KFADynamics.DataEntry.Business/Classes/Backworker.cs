#region Imports

using KFADynamics.DataEntry.Business.Delegates;
using System.ComponentModel;

#endregion

namespace KFADynamics.DataEntry.Business.Classes;

public class BackgroundWorkHelper
{
  private readonly ValueMonitor<int> _percentageProgress = new(0);
  private readonly ValueMonitor<TimeSpan> _timeLeft = new(TimeSpan.MaxValue);
  private DateTime _startTime;
  private List<Action>? _toDo;
  private readonly BackgroundWorker _worker = new();
  ErrorHandler? _errorHandler;

  public BackgroundWorkHelper(ErrorHandler? errorHandler)
  {
    _errorHandler = errorHandler;
  }

  public BackgroundWorkHelper()
  {
    IsParallel = false;
    BackgroundWorker.WorkerReportsProgress = true;
    BackgroundWorker.WorkerSupportsCancellation = true;
    _percentageProgress.ValueChanged += percentageProgress_ValueChanged;

    BackgroundWorker.DoWork += worker_DoWork;
  }

  public BackgroundWorkHelper(List<Action> actionsToDo)
      : this()
  {
    _toDo = actionsToDo;
  }

  public BackgroundWorker BackgroundWorker => _worker;

  public bool IsParallel { get; set; }

  public IValueMonitor<TimeSpan> TimeLeft => _timeLeft;

  public int Total => _toDo == null ? 0 : _toDo.Count;

  public void SetActionsTodo(List<Action> toDoActions, bool cancelCurrent = false)
  {
    if (BackgroundWorker.IsBusy && cancelCurrent)
      BackgroundWorker.CancelAsync();

    BackgroundWorker.DoWork -= worker_DoWork;
    BackgroundWorker.DoWork += worker_DoWork;
    BackgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
    _toDo = toDoActions;
  }

  private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs? e)
  {
    (sender as BackgroundWorker)?.Dispose();
  }

  private void percentageProgress_ValueChanged(int oldValue, int newValue)
  {
    BackgroundWorker.ReportProgress(newValue);
  }

  private void worker_DoWork(object? sender, DoWorkEventArgs? e)
  {
    if (_toDo == null) throw new InvalidOperationException("You must provide actions to execute");
    Thread.Sleep(10);
    var total = _toDo.Count;
    _startTime = DateTime.Now;
    var current = 0;

    if (IsParallel == false)
      foreach (var next in _toDo)
      {
        next();
        current++;
        if (_worker?.CancellationPending ?? false) return;
        _percentageProgress.Value = (int)(current / (double)total * 100.0);
        var passedMs = (DateTime.Now - _startTime).TotalMilliseconds;
        var oneUnitMs = passedMs / current;
        var leftMs = (total - current) * oneUnitMs;
        _timeLeft.Value = TimeSpan.FromMilliseconds(leftMs);
      }
    else
      try
      {
        Parallel.For(0, total,
            (index, loopstate) =>
            {
              _toDo.ElementAt(index)();
              if (_worker?.CancellationPending??false) loopstate.Stop();
              Interlocked.Increment(ref current);

              _percentageProgress.Value = (int)(current / (double)total * 100.0);
              var passedMs = (DateTime.Now - _startTime).TotalMilliseconds;
              var oneUnitMs = passedMs / current;
              var leftMs = (total - current) * oneUnitMs;
              _timeLeft.Value = TimeSpan.FromMilliseconds(leftMs);
            });
      }
      catch (Exception ex)
      {
        _errorHandler?.Invoke("Background Action Error", ex.Message, ex);
      }
  }
}
