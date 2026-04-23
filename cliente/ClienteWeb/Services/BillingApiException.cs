using System;

namespace ClienteWeb.Services
{
    public class BillingApiException : Exception
    {
        public int StatusCode { get; }

        public BillingApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public BillingApiException(string message, int statusCode, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
