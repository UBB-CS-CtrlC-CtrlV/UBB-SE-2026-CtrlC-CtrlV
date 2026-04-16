using BankApp.Client.Utilities;

namespace BankApp.Client.Tests;

internal class FakeCountdownTimer : ICountdownTimer
{
    public event EventHandler? Tick;
    public bool IsRunning { get; private set; }

    public void Start() => this.IsRunning = true;
    public void Stop() => this.IsRunning = false;
    public void FireTick() => this.Tick?.Invoke(this, EventArgs.Empty);
}