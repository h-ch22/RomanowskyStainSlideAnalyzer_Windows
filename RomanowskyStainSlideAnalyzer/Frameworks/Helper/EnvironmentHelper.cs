using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Text;
using Windows.Devices.Sensors;
using Windows.Storage;

namespace RomanowskyStainSlideAnalyzer.Frameworks.Helper
{
    class EnvironmentHelper
    {
        public bool isVirtualMachinePlatformActivated = false;
        public bool isWSLActivated = false;

        private readonly string featureActivatorPath = "C:\\Program Files\\Romanowsky Stain Slide Analyzer\\Romanowsky Stain Slide Analyzer Feature Activator";
        private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

        public bool GetWSLStatus()
        {
            using ManagementClass objMC = new("Win32_OptionalFeature");
            using ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                string featureName = (string)objMO.Properties["Caption"].Value;

                if (featureName.ToLower().Contains("linux") || featureName.ToLower() == "virtual machine platform")
                {
                    bool isEnabled = objMO.Properties["InstallState"].Value.ToString() == "1";

                    if(featureName.ToLower() == "virtual machine platform")
                    {
                        isVirtualMachinePlatformActivated = isEnabled;
                    } else if(featureName.ToLower().Contains("linux"))
                    {
                        isWSLActivated = isEnabled;
                        updateSettings("WSL_Status", isEnabled);
                    }
                }

                objMO.Dispose();
            }

            updateSettings("Windows_Additional_Features_Status", isVirtualMachinePlatformActivated);
            return isWSLActivated && isVirtualMachinePlatformActivated;
        }

        public bool GetFeatureActivatorInstalledStatus()
        {
            var isExists = Directory.Exists(featureActivatorPath);
            updateSettings("Feature_Activator_Installation_Status", isExists);
            return isExists;
        }

        public bool InstallFeatureActivator()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Include\setup.exe");
            Process process = new();
            process.StartInfo.FileName = path;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";

            try
            {
                process.Start();
                process.WaitForExit();

                updateSettings("Feature_Activator_Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                return false;
            }
        }

        public bool ActivateFeatures(int code=0)
        {
            try
            {
                Process process = new();
                process.StartInfo.FileName = $"{featureActivatorPath}\\FeatureInstaller.exe";
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Arguments = code == 0 ? "/Install-Windows-Additional-Features" : "/Install-Ubuntu";
                process.StartInfo.Verb = "runas";

                process.Start();
                process.WaitForExit();

                var exitCode = process.ExitCode;
                updateSettings(code == 0 ? "Windows_Additional_Features_Status" : "Linux_Installation_Status", exitCode == 0);

                return exitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public void reboot(int t = 1)
        {
            Process.Start("shutdown.exe", $"-r -t {t}");
        }

        public bool GetInstalledLinux()
        {
            try
            {
                var updateWSL = runCommand("powershell.exe", "wsl --update");
                var installedLinux = runCommand("powershell.exe", "wsl -l -q | more");
                var installedLinuxList = installedLinux.Split("\n");

                foreach(var dist in installedLinuxList)
                {
                    if (dist.ToLower() == "ubuntu")
                    {
                        updateSettings("Linux_Installation_Status", true);
                        return true;
                    }
                }

                updateSettings("Linux_Installation_Status", false);
                return false;

            } catch(Exception e)
            {
                updateSettings("Linux_Installation_Status", false);
                Debug.WriteLine($"Exception at GetInstalledLinux(): {e.Message}");
                return false;
            }
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
                        CreateNoWindow = true,
                    }
                };

                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                output = output.Replace("\r\n\r\n\r\n\r\n\r\n", "\n");
                output = output.Replace("\r\n", "");

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new ArgumentException("Process Not Exited Successfully.");
                }

                return output;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        private bool runLinux(string arguments)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ubuntu",
                        Arguments = $"run {arguments}",
                        UseShellExecute = true,
                        CreateNoWindow = false,
                    }
                };

                process.Start();

                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public bool InstallUbuntu()
        {
            return ActivateFeatures(1);
        }

        public bool GetGPU()
        {
            var gpuList = runCommand("powershell", "(Get-WmiObject Win32_VideoController).Name");

            return gpuList.Contains("NVIDIA");
        }

        public bool InstallEssentialPackages()
        {
            var result = runLinux("sudo apt-get update && sudo apt-get -y install gcc g++ python3 python3-pip python3.12-venv");

            if(!result) { return false; }

            var downloadResult = runLinux("cd ~ && rm -rf Apps && mkdir Apps && cd Apps && wget https://developer.download.nvidia.com/compute/cuda/repos/wsl-ubuntu/x86_64/cuda-wsl-ubuntu.pin && wget https://developer.download.nvidia.com/compute/cudnn/9.5.1/local_installers/cudnn-local-repo-ubuntu2404-9.5.1_1.0-1_amd64.deb");

            if(!downloadResult) { return false; }

            var cudaInstallResult = runLinux("cd ~/Apps && sudo mv cuda-wsl-ubuntu.pin /etc/apt/preferences.d/cuda-repository-pin-600 && wget https://developer.download.nvidia.com/compute/cuda/12.6.3/local_installers/cuda-repo-wsl-ubuntu-12-6-local_12.6.3-1_amd64.deb && sudo dpkg -i cuda-repo-wsl-ubuntu-12-6-local_12.6.3-1_amd64.deb && sudo cp /var/cuda-repo-wsl-ubuntu-12-6-local/cuda-*-keyring.gpg /usr/share/keyrings/ && sudo apt-get update && sudo apt-get -y install cuda-toolkit-12-6");

            if (!cudaInstallResult) { return false; }

            var cudnnInstallResult = runLinux("cd ~/Apps && sudo dpkg -i cudnn-local-repo-ubuntu2404-9.5.1_1.0-1_amd64.deb && sudo cp /var/cudnn-local-repo-ubuntu2404-9.5.1/cudnn-*-keyring.gpg /usr/share/keyrings/ && sudo apt-get update && sudo apt-get -y install cudnn cudnn-cuda-12");

            if(!cudnnInstallResult) { return false; }

            updateSettings("Essential_Packages_Status", true);

            return true;
        }

        public bool DownloadSAMProject()
        {
            var result = runLinux("cd ~ && rm -rf RomanowskyStainSlideAnalyzer && mkdir RomanowskyStainSlideAnalyzer");

            if (!result) { return false; }

            var downloadResult = runLinux("cd ~/RomanowskyStainSlideAnalyzer && git clone https://github.com/facebookresearch/sam2.git");

            if(!downloadResult) { return false; }
            updateSettings("Project_Status", true);

            return true;
        }

        public bool CreateVirtualEnv()
        {
            var result = runLinux("cd ~ && cd RomanowskyStainSlideAnalyzer && rm -rf RomanowskyStainSlideAnalyzer_venv && python3 -m venv RomanowskyStainSlideAnalyzer_venv");

            return result;
        }

        public bool InstallPythonPackages()
        {
            var result = runLinux("cd ~/RomanowskyStainSlideAnalyzer && source RomanowskyStainSlideAnalyzer_venv/bin/activate && pip install torch torchvision torchaudio opencv-python matplotlib pillow && cd sam2 && cd checkpoints && ./download_ckpts.sh");

            updateSettings("Python_Packages_Status", true);

            return true;
        }

        public bool CopyEntryPoint()
        {
            var result = runLinux("cd ~/RomanowskyStainSlideAnalyzer && mv ~/RomanowskyStainSlideAnalyzer/sam2 ~/RomanowskyStainSlideAnalyzer/include && cp -r ~/RomanowskyStainSlideAnalyzer/include/sam2/configs/ ~/RomanowskyStainSlideAnalyzer/ && cp -r ~/RomanowskyStainSlideAnalyzer/include/checkpoints/ ~/RomanowskyStainSlideAnalyzer/ && source RomanowskyStainSlideAnalyzer_venv/bin/activate && cd include && pip install -e .");

            if (!result) { return false; }

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Include");

            path = path.Replace(@"\", "/").Replace(@"C:/", "c/").Replace("Program Files", @"Program\ Files");

            var cpResult = runLinux($"cp /mnt/{path}/main.py ~/RomanowskyStainSlideAnalyzer/");

            if (!cpResult)
            {
                return false;
            }

            updateSettings("Entry_Point_Status", true);
            updateSettings("Entry_Point_Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            return cpResult;
        }

        public bool UpdateEntryPoint()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Include");
            path = path.Replace(@"\", "/").Replace(@"C:/", "c/").Replace("Program Files", @"Program\ Files");

            var cpResult = runLinux($"cp /mnt/{path}/main.py ~/RomanowskyStainSlideAnalyzer/");

            if (!cpResult)
            {
                return false;
            }

            updateSettings("Entry_Point_Status", true);
            updateSettings("Entry_Point_Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            return cpResult;
        }

        private void updateSettings(string key, bool value)
        {
            settings.Values[key] = value;
        }

        private void updateSettings(string key, string value)
        {
            settings.Values[key] = value;
        }

        public void UpdateLastLaunchedVersion()
        {
            settings.Values["Last_Launched_Version"] = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public bool GetStatus(string key)
        {
            switch (key)
            {
                case "WSL Status": return settings.Values["WSL_Status"] as bool? ?? false;
                case "Windows Additional Features Status": return settings.Values["Windows_Additional_Features_Status"] as bool? ?? false;
                case "Feature Activator Status": return settings.Values["Feature_Activator_Installation_Status"] as bool? ?? false;
                case "Linux Status": return settings.Values["Linux_Installation_Status"] as bool? ?? false;
                case "Essential Packages Status": return settings.Values["Essential_Packages_Status"] as bool? ?? false;
                case "Python Packages Status": return settings.Values["Python_Packages_Status"] as bool? ?? false;
                case "Project Status": return settings.Values["Project_Status"] as bool? ?? false;
                case "Entry Point Status": return settings.Values["Entry_Point_Status"] as bool? ?? false;

                default: return false;
            }
        }

        public string GetLibrariesVersion(string key)
        {
            switch (key)
            {
                case "Entry Point Version": return settings.Values["Entry_Point_Version"] as string ?? "1.0.0.0";
                case "Feature Activator Version": return settings.Values["Feature_Activator_Version"] as string ?? "1.0.0.0";
                default: return "";
            }
        }

        public string GetLastLaunchedVersion()
        {
            return settings.Values["Last_Launched_Version"] as string ?? "1.0.0.0";
        }

        public bool GetFinalStatus()
        {
            var wslStatus = GetStatus("WSL Status");
            var addtionalFeaturesStatus = GetStatus("Windows Additional Features Status");
            var featureActivatorStatus = GetStatus("Feature Activator Status");
            var linuxStatus = GetStatus("Linux Status");
            var essentialPackagesStatus = GetStatus("Essential Packages Status");
            var pythonPackagesStatus = GetStatus("Python Packages Status");
            var projectStatus = GetStatus("Project Status");
            var entryPointStatus = GetStatus("Entry Point Status");

            var result = wslStatus && addtionalFeaturesStatus && featureActivatorStatus && linuxStatus && essentialPackagesStatus && pythonPackagesStatus && projectStatus && entryPointStatus;

            updateSettings("Entry_Point_Status", result);

            return result;
        }
    }
}
