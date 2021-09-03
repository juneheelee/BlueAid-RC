using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueAid_RC.Model
{
    public class User
    {
        public string userName { get; set; }

        public string userNumber { get; set; }

        public User(string name, string number)
        {
            this.userName = name;
            this.userNumber = number;
        }
    }
}
