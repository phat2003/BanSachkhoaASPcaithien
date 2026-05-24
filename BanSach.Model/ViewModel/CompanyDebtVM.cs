using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanSach.Models.ViewModel
{
    public class CompanyDebtVM
    {
        public Company company { get; set; }
        public double TotalDebt { get; set; }
        public OrderHeader orderHeader { get; set; }
    }
}
