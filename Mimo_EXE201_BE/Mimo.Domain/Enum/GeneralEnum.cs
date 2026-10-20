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
            Active = 1,
            Disable = 2,
            Pending = 3,
        }

        public enum UserStatusEnum
        {
            Active = 1,
            Disable = 2,
            Archived = 3,
            NotVerified = 4,
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

        public enum SubscriptionTierEnum
        {
            Free = 1,
            Standard = 2,
            Premium = 3,
            Enterprise = 4
        }

        public enum DifficultyLevelEnum
        {
            Beginner = 1,
            Intermediate = 2,
            Advanced = 3
        }

        public enum CourseStatusEnum
        {
            Draft = 1,
            Published = 2,
            Archived = 3
        }

        public enum LessonContentTypeEnum
        {
            Video = 1,
            InteractiveSimulation = 2,
            Quiz = 3,
            Text = 4
        }

        public enum EnrollmentStatusEnum
        {
            Active = 1,
            Completed = 2,
            Dropped = 3
        }

        public enum LessonProgressStatusEnum
        {
            NotStarted = 1,
            InProgress = 2,
            Completed = 3
        }

        public enum PaymentMethodEnum
        {
            CreditCard = 1,
            BankTransfer = 2,
            VNPay = 3,
            MoMo = 4,
            Cash = 5
        }

        public enum TransactionStatusEnum
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Refunded = 4
        }
    }
}

