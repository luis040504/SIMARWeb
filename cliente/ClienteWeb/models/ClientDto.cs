using System;

namespace ClienteWeb.Models
{
    public class ClientDto
    {
        public string Name { get; set; }
        public string BusinessName { get; set; }

        public string Alias { get; set; }
        public string ContactEmail { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string RFC { get; set; }
        public string SemarnatNum { get; set; }
    }

    public class ClienteOutput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BusinessName { get; set; }

        public string Alias { get; set; }
        public string Phone { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string RFC { get; set; }
        public string UrlSatCertificate { get; set; }
        public string SemarnatNum { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ContactEmail { get; set; }
        public string IdUser { get; set; }
    }
}