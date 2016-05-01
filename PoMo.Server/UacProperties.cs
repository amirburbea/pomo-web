using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace PoMo.Server
{
    internal static class UacProperties
    {
        private enum TokenElevationType
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        private enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        public static bool IsProcessElevated
        {
            get
            {
                if (!UacProperties.IsUacEnabled)
                {
                    WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    return principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(0x200); //Domain Administrator
                }
                IntPtr tokenHandle;
                if (!NativeMethods.OpenProcessToken(Process.GetCurrentProcess().Handle, 0x00020000 | 0x0008, out tokenHandle))
                {
                    throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
                }
                try
                {
                    int elevationResultSize = Marshal.SizeOf((int)TokenElevationType.TokenElevationTypeDefault);
                    IntPtr elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);
                    try
                    {
                        uint returnedSize;
                        bool success = NativeMethods.GetTokenInformation(
                            tokenHandle,
                            TokenInformationClass.TokenElevationType,
                            elevationTypePtr,
                            (uint)elevationResultSize,
                            out returnedSize
                        );
                        if (!success)
                        {
                            throw new ApplicationException("Unable to determine the current elevation.");
                        }
                        return (TokenElevationType)Marshal.ReadInt32(elevationTypePtr) == TokenElevationType.TokenElevationTypeFull;
                    }
                    finally
                    {
                        if (elevationTypePtr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(elevationTypePtr);
                        }
                    }
                }
                finally
                {
                    if (tokenHandle != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(tokenHandle);
                    }
                }
            }
        }

        public static bool IsUacEnabled
        {
            get
            {
                const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
                using (RegistryKey uacKey = Registry.LocalMachine.OpenSubKey(uacRegistryKey, false))
                {
                    const string uacRegistryValue = "EnableLUA";
                    return uacKey != null && object.Equals(uacKey.GetValue(uacRegistryValue), 1);
                }
            }
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr handle);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength);

            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
        }
    }
}