using BankApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Core.DTOs.Profile
{
    public class GetProfileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ProfileInfo? ProfileInfo { get; set; }

        public GetProfileResponse() { }

        public GetProfileResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public GetProfileResponse(bool success, string message, User user)
        {
            Success = success;
            Message = message;
            ProfileInfo = new ProfileInfo(user);
        }
    }
}

