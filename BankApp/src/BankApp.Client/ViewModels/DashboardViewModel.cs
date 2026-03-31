using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Dashboard;
using BankApp.Core.Entities;
using BankApp.Core.Enums;
using System;
using System.Collections.Generic;


namespace BankApp.Client.ViewModels
{
    public class DashboardViewModel 
    {
        public ObservableState<DashboardState> State { get; private set; }
        public User CurrentUser { get; private set; }
        public List<Card> Cards { get; private set; }
        public List<Transaction> RecentTransactions { get; private set; }
        public int UnreadNotificationCount { get; private set; }

        private readonly ApiClient _apiClient;

        public DashboardViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            State = new ObservableState<DashboardState>(DashboardState.Loading);
            Cards = new List<Card>();
            RecentTransactions = new List<Transaction>();
            UnreadNotificationCount = 0;
        }

        public async void LoadDashboard()
        {
            State.SetValue(DashboardState.Loading);
            try
            {
                int? userId = _apiClient.GetCurrentUserId();
                if (userId == null)
                {
                    State.SetValue(DashboardState.Error);
                    return;
                }

                DashboardResponse? response = await _apiClient.GetAsync<DashboardResponse>(
                    $"/api/dashboard/");

                if (response == null)
                {
                    State.SetValue(DashboardState.Error);
                    return;
                }

                CurrentUser = response.CurrentUser;
                Cards = response.Cards;
                RecentTransactions = response.RecentTransactions;
                UnreadNotificationCount = response.UnreadNotificationCount;
                State.SetValue(DashboardState.Success);
            }
            catch (Exception)
            {
                State.SetValue(DashboardState.Error);
            }
        }

        public void Dispose() { }
    }
}


