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
        public enum UserRoleEnum
        {
            Admin = 1,
            Student = 2,
            Curator = 3,
            Organization = 4
        }
        public enum OrganizationStatusEnum
        {
            Active = 1,
            Pending = 2,
            Suspended = 3,
            Expired = 4
        }
    }
}
