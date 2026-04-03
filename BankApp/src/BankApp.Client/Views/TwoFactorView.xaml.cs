// <copyright file="TwoFactorView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Core.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    /// <summary>
    /// Displays the OTP verification step of the login flow.
    /// </summary>
    public sealed partial class TwoFactorView : Page, IStateObserver<TwoFactorState>
    {
        private readonly DispatcherTimer countdownTimer;
        private readonly TwoFactorViewModel viewModel;
        private int secondsRemaining = 30;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFactorView"/> class.
        /// </summary>
        public TwoFactorView()
        {
            this.InitializeComponent();

            this.viewModel = new TwoFactorViewModel(App.ApiClient);
            this.viewModel.State.AddObserver(this);

            this.countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            this.countdownTimer.Tick += this.CountdownTimerTick;
        }

        /// <inheritdoc/>
        public void Update(TwoFactorState state)
        {
            this.OnStateChanged(state);
        }

        private void OnStateChanged(TwoFactorState state)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                this.HideLoading();
                this.ErrorInfoBar.IsOpen = false;

                switch (state)
                {
                    case TwoFactorState.Idle:
                        break;

                    case TwoFactorState.Verifying:
                        this.ShowLoading();
                        break;

                    case TwoFactorState.Success:
                        App.NavigationService.NavigateTo<NavView>();
                        break;

                    case TwoFactorState.InvalidOTP:
                        this.ShowError("The code you entered is incorrect.");
                        break;

                    case TwoFactorState.Expired:
                        this.ShowError("This code has expired. Please request a new one.");
                        break;

                    case TwoFactorState.MaxAttemptsReached:
                        this.ShowError("Maximum attempts reached. Your account has been locked.");
                        this.VerifyButton.IsEnabled = false;
                        this.OtpBox.IsEnabled = false;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            });
        }

        private void ShowError(string msg)
        {
            this.ErrorInfoBar.Message = msg;
            this.ErrorInfoBar.IsOpen = true;
        }

        private void ShowLoading()
        {
            this.LoadingRing.IsActive = true;
            this.LoadingRing.Visibility = Visibility.Visible;
            this.VerifyButton.IsEnabled = false;
            this.OtpBox.IsEnabled = false;
        }

        private void HideLoading()
        {
            this.LoadingRing.IsActive = false;
            this.LoadingRing.Visibility = Visibility.Collapsed;
            this.VerifyButton.IsEnabled = true;
            this.OtpBox.IsEnabled = true;
        }

        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            string otp = this.OtpBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(otp) || otp.Length != 6)
            {
                this.ShowError("Please enter a valid 6-digit code.");
                return;
            }

            await this.viewModel.VerifyOtp(otp);
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            this.ResendButton.IsEnabled = false;
            this.secondsRemaining = 30;
            this.CountdownText.Text = $"Available in {this.secondsRemaining}s";
            this.CountdownText.Visibility = Visibility.Visible;

            this.countdownTimer.Start();
            await this.viewModel.ResendOtp();
        }

        private void CountdownTimerTick(object? sender, object e)
        {
            this.secondsRemaining--;

            if (this.secondsRemaining <= 0)
            {
                this.countdownTimer.Stop();
                this.ResendButton.IsEnabled = true;
                this.CountdownText.Visibility = Visibility.Collapsed;
                return;
            }

            this.CountdownText.Text = $"Available in {this.secondsRemaining}s";
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            App.ApiClient.ClearToken();
            App.NavigationService.NavigateTo<LoginView>();
        }
    }
}
