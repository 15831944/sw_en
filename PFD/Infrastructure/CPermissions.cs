﻿using BaseClasses;
using MATH;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PFD
{
    public static class CPermissions
    {
        private static Dictionary<EUserPermission, bool> Permissions;

        public static bool UserHasPermission(EUserPermission permission)
        {
            if (Permissions == null) return false;

            return Permissions[permission];
        }
        public static void InitPermissions(string role)
        {
            //init all to false
            Permissions = new Dictionary<EUserPermission, bool>()
            {
                { EUserPermission.ViewTabGeneral, false },
                { EUserPermission.ViewTabMember_Input, false },
                { EUserPermission.ViewTabDoorsAndWindows, false },
                { EUserPermission.ViewTabJoint_Input, false },
                { EUserPermission.ViewTabFooting_Input, false },
                { EUserPermission.ViewTabLoads, false },
                { EUserPermission.ViewTabLoadCases, false },
                { EUserPermission.ViewTabLoadCombinations, false },
                { EUserPermission.ViewTabInternalForces, false },
                { EUserPermission.ViewTabMemberDesign, false },
                { EUserPermission.ViewTabJointDesign, false },
                { EUserPermission.ViewTabFootingDesign, false },
                { EUserPermission.ViewTabPartList, false },
                { EUserPermission.ViewTabQuoation, false }
            };

            if (role.Equals("developer", StringComparison.OrdinalIgnoreCase))
            {
                AddAllPermissions();
            }
            else if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                //atd
                Permissions[EUserPermission.ViewTabGeneral] = true;
            }
        }

        private static void AddAllPermissions()
        {
            Permissions.Keys.ToList().ForEach(k => Permissions[k] = true);            
        }
    }

    public enum EUserPermission
    {
        ViewTabGeneral = 0,
        ViewTabMember_Input = 1,
        ViewTabDoorsAndWindows = 2,
        ViewTabJoint_Input = 3,
        ViewTabFooting_Input = 4,
        ViewTabLoads = 5,
        ViewTabLoadCases = 6,
        ViewTabLoadCombinations = 7,
        ViewTabInternalForces = 8,
        ViewTabMemberDesign = 9,
        ViewTabJointDesign = 10,
        ViewTabFootingDesign = 11,
        ViewTabPartList = 12,
        ViewTabQuoation = 13
    }
}
