using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WpfReportLauncher.Models
{
    internal class SettingItems
    {
        public class AppSettings
        {
            public OutputSettings Output { get; set; }
            public InvoiceSettings Invoice { get; set; }
        }

        public class OutputSettings
        {
            public string BaseDirectory { get; set; }
        }

        public class InvoiceSettings
        {
            public string CompanyName { get; set; }
            public string IssuerName { get; set; }
            public double TaxRate { get; set; }
        }
    }
}
