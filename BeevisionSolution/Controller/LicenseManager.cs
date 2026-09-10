using CodeMeter;

namespace BeevisionSolution.Controller
{
    /// <summary>
    /// Check license
    /// </summary>
    public static class LicenseManager
    {
        private const ushort AnyBoxMask = 0;
        private const uint AnySerial = 0;

        private static readonly Api Cm = new Api();

        public static bool HasLicense(uint firmCode, uint productCode)
        {
            HCMSysEntry handle = null;
            try
            {
                var access = new CmAccess2
                {
                    BoxMask = AnyBoxMask,
                    SerialNumber = AnySerial,
                    FirmCode = firmCode,
                    ProductCode = productCode,
                };

                access.Ctrl |= CmAccess.Option.NoUserLimit;

                handle = Cm.CmAccess2(CmAccessOption.Local, access);
                return handle != null;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (handle != null)
                {
                    try { Cm.CmRelease(handle); } catch { }
                }
            }
        }
    }
}
