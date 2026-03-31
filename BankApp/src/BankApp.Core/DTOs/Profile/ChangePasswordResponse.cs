using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Core.DTOs.Profile
{
    public class ChangePasswordResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public ChangePasswordResponse() {  }

        public ChangePasswordResponse(bool success, string? message)
        {
            Success = success;
            Message = message;
        }
    }
}

