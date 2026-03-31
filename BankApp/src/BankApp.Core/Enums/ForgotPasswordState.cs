namespace BankApp.Core.Enums
{
    public enum ForgotPasswordState { Idle, EmailSent, TokenValid, TokenExpired, TokenAlreadyUsed, PasswordResetSuccess, Error }
}
