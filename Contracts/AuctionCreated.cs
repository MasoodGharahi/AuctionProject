using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class AuctionCreated
    {
        public string Id { get; set; }
        public string Make { get; set; }

        public string Model { get; set; }

        public int Year { get; set; }

        public string Color { get; set; }

        public int Mileage { get; set; }

        public string Image { get; set; }

        public Int64 ReservePrice { get; set; }

        public DateTime AuctionEnd { get; set; }
    }
}
