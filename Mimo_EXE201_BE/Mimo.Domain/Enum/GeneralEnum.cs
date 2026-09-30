using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimo.DomainLayer.Enum
{
    public class GeneralEnum
    {
        public enum RoleStatusEnum
        {
            Active,
            Disable,
            Pending,
        }
        public enum UserStatusEnum
        {
            Active,
            Disable,
            Archived,
            NotVerified,
        }
    }
}
