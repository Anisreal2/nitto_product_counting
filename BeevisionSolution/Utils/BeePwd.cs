using Microsoft.DwayneNeed.Win32.User32;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Utils
{
    internal class BeePwd
    {
        public static bool CreatMasterPassword(string pwd)
        {
            try
            {
                RegistryKey key = CreatAppRegistry();

                if (key != null)
                {
                    key.SetValue("M", pwd);
                    return true;
                }

            }
            catch (Exception ex)
            {
                Common.Bug(ex.Message);
            }

            return false;
        }

        public static bool CreatEngineerPassword(string pwd)
        {
            try
            {
                RegistryKey key = CreatAppRegistry();

                if (key != null)
                {
                    key.SetValue("E", pwd);
                    return true;
                }

            }
            catch (Exception ex)
            {
                Common.Bug(ex.Message);
            }

            return false;
        }

        public static string GetMasterPassword()
        {
            try
            {
                RegistryKey key = CreatAppRegistry();

                if (key != null)
                {
                    var keyM = key.GetValue("M");
                    if (keyM == null)
                        return "";
                    else
                    {
                        var pwd = key.GetValue("M").ToString();
                        return pwd;
                    }
                }

            }
            catch (Exception ex)
            {
                Common.Bug(ex.Message);
            }
            return "";
        }

        public static string GetEngineerPassword()
        {
            try
            {
                RegistryKey key = CreatAppRegistry();

                if (key != null)
                {
                    var keyE = key.GetValue("E");
                    if (keyE == null)
                        return "";
                    else
                    {
                        var pwd = key.GetValue("E").ToString();
                        return pwd;
                    }

                }

            }
            catch (Exception ex)
            {
                Common.Bug(ex.Message);
            }
            return "";
        }

        private static RegistryKey CreatAppRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

            var ret = key.OpenSubKey("BeeVision", true);
            if (ret == null)
            {
                ret = key.CreateSubKey("BeeVision", true);
                return ret;
            }
            return ret;
        }

        public static bool IsMasterPwdCreated()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", false);

            key = key.OpenSubKey("BeeVision", true);
            if (key == null)
            {
                return false;
            }
            else
            {
                if (key.GetValue("M") == null)
                {
                    return false;
                }
            }
            return true;
        }


    }
}
