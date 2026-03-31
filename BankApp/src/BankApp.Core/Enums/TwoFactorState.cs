namespace BankApp.Core.Enums
{
    public enum TwoFactorState { Idle, Verifying, Success, InvalidOTP, Expired, MaxAttemptsReached }
}
