using System;
using System.Diagnostics;
using System.Management;
using Windows.Devices.Sensors;

namespace RomanowskyStainSlideAnalyzer.Frameworks.Helper
{
    class EnvironmentHelper
    {
        public bool isVirtualMachinePlatformActivated = false;
        public bool isWSLActivated = false;
        public bool isHyperVActivated = false;

        private string[] linuxDistros = { "Ubuntu", "Debian", "Kali", "Fedora", "openSUSE", "Alpine" };

        public bool GetWSLStatus()
        {
            using ManagementClass objMC = new("Win32_OptionalFeature");
            using ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                string featureName = (string)objMO.Properties["Caption"].Value;

                if (featureName.ToLower().Contains("linux") || featureName.ToLower() == "virtual machine platform" || featureName == "Hyper-V")
                {
                    bool isEnabled = objMO.Properties["InstallState"].Value.ToString() == "1";

                    if(featureName.ToLower() == "virtual machine platform")
                    {
                        isVirtualMachinePlatformActivated = isEnabled;
                    } else if(featureName.ToLower().Contains("linux"))
                    {
                        isWSLActivated = isEnabled;
                    } else if(featureName == "Hyper-V")
                    {
                        isHyperVActivated = isEnabled;
                    }
                }

                objMO.Dispose();
            }

            return isWSLActivated && isVirtualMachinePlatformActivated && isHyperVActivated;
        }

        public string? GetInstalledLinux()
        {
            try
            {
                var output = runCommand("wsl", "--list --verbose");

                foreach(var distro in linuxDistros)
                {
                    if (output.ToLower().Contains(distro.ToLower(), StringComparison.OrdinalIgnoreCase))
                    {
                        return distro;
                    }
                }

                return "";

            } catch(Exception e)
            {
                Debug.WriteLine($"Exception at GetInstalledLinux(): {e.Message}");
                return null;
            }
        }

        public bool InstallUbuntu()
        {
            try
            {
                string result = runCommand("wsl", "--install -d Ubuntu");

                if (!string.IsNullOrEmpty(result))
                {
                    if (result.Contains("The requested operation is successful", StringComparison.OrdinalIgnoreCase) ||
                        result.Contains("Ubuntu is already installed", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("Ubuntu가 성공적으로 설치되었거나 이미 설치되어 있습니다.");
                        return true;
                    }
                    else if (result.Contains("requires administrator privileges", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("WSL 설치 명령어는 관리자 권한이 필요합니다. 프로그램을 관리자 모드로 실행하세요.");
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("Ubuntu 설치 중 알 수 없는 문제가 발생했습니다.");
                        Debug.WriteLine($"출력 내용: {result}");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception at InstallUbuntu(): {e.Message}");
                return false;
            }

        }

        public void reboot(int t=1)
        {
            Process.Start("shutdown.exe", $"-r -t {t}");
        }

        private string runCommand(string command, string arguments)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = false
                    }
                };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Debug.WriteLine(output);

                return output;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
