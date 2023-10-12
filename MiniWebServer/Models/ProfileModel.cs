using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Models
{
    internal class ProfileModel
    {
        public required string Name { get; set; }
        public required int YearOfBirth { get; set; }
    }
}
