using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberAPI
{
    public static class Permissions
    {
        public static uint ToUint(this string value)
        {

            uint result = 0;
            uint mult = 1;
            char[] arr = value.ToCharArray();
            Array.Reverse(arr);
            foreach (Char c in arr)
            {
                if (c == '1') result += mult;
                mult *= 2;
            }
            return result;
        }

        public static bool IsRole(this uint permission, PermissionRole role)
        {
            return (permission & (uint)role) != 0;
        }

        public enum PermissionRole : uint
        {
            p_Admin =       0b10000000000000000000000000000000,
            p_Pool =        0b01000000000000000000000000000000,
            p_SeeAllUsers = 0b00100000000000000000000000000000,
            p_Test =        0b11111111111111111111111111111111
        }
    }
}
