using System.Globalization;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Helix.Notebooks.Core;

public class PythonKernel : IPythonKernel
{
    public PythonKernel()
    {
        if (!PythonEngine.IsInitialized)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string enginePath = Path.Combine(baseDirectory, "python_engine");
            string pythonDllPath = Path.Combine(enginePath, "python311.dll");
            
            string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            
            if (!currentPath.Contains(enginePath))
            {
                Environment.SetEnvironmentVariable("PATH", enginePath + ";" + currentPath);
            }
            
            Environment.SetEnvironmentVariable("PYTHONHOME", enginePath);
            Runtime.PythonDLL = pythonDllPath;
            
            if (!File.Exists(pythonDllPath))
            {
                throw new FileNotFoundException($"CRITICAL BUILD ERROR: The Python engine is physically missing from the output directory! Expected to find it here: {pythonDllPath}");
            }
            
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }

    public PyModule CreateScope()
    {
        using (Py.GIL())
        {
            return Py.CreateScope();
        }
    }

    public async Task SetWorkingDirectoryAsync(string fullFilePath, PyModule scope)
    {
        if (string.IsNullOrWhiteSpace(fullFilePath)) return;
    
        string directory = Path.GetDirectoryName(fullFilePath) ?? string.Empty;

        if (Directory.Exists(directory))
        {
            // We use a raw string (r'') to handle Windows backslashes correctly
            string code = $"import os\nos.chdir(r'{directory}')";
        
            await Task.Run(() => Execute(code, scope));
        }
    }

    public string Execute(string pythonCode, PyModule scope)
    {
        using (Py.GIL())
        {
            try
            {
                // Standard I/O redirection to capture print() and errors
                string redirectCode = @"
import sys
import io
sys.stdout = io.StringIO()
sys.stderr = sys.stdout
";

                scope.Exec(redirectCode);
                scope.Exec(pythonCode);

                var output = scope.Eval("sys.stdout.getvalue()").ToString(CultureInfo.InvariantCulture);

                return string.IsNullOrWhiteSpace(output) ? "[Executed without output]" : output.TrimEnd();
            }
            catch (PythonException e)
            {
                return $"Python error:\n{e.Message}";
            }
            catch (Exception e)
            {
                return $"Engine error:\n{e.Message}";
            }
        }
    }

    public Dictionary<string, string> GetInstalledPackages()
    {
        var packages = new Dictionary<string, string>();

        using (Py.GIL())
        {
            try
            {
                using (var tempScope = Py.CreateScope())
                {
                    string code = @"
import sys
import os
import importlib.metadata

# 1. Explicitly inject the site-packages path for the bundled engine
site_packages = os.path.join(sys.prefix, 'Lib', 'site-packages')
if os.path.exists(site_packages) and site_packages not in sys.path:
    sys.path.append(site_packages)

def get_packages():
    result = {}
    try:
        # 2. Safely extract package names and versions
        for dist in importlib.metadata.distributions():
            name = dist.metadata.get('Name') or getattr(dist, 'name', 'Unknown')
            version = getattr(dist, 'version', 'Unknown')
            if name:
                result[name] = version
    except Exception as e:
        result['PYTHON_ERROR'] = str(e)
    return result
";
                    tempScope.Exec(code);
                    dynamic get_packages_func = tempScope.Get("get_packages");
                
                    using (PyDict pyDict = new PyDict(get_packages_func()))
                    {
                        foreach (PyObject key in pyDict.Keys())
                        {
                            packages[key.As<string>()] = pyDict[key].As<string>();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // 3. Surface C# crashes directly to the UI
                packages["C#_CRITICAL_ERROR"] = e.Message;
            }
        }

        return packages;
    }

    public async Task<string> InstallPackagesAsync(string packageName)
    {
        return await Task.Run(() =>
        {
            using (Py.GIL())
            {
                try
                {
                    using (var tempScope = Py.CreateScope())
                    {
                        string pythonExe = Path.Combine(Environment.GetEnvironmentVariable("PYTHONHOME") ?? "", "python.exe");
                    
                        string code = $@"
import subprocess
def run_pip_install(pkg):
    # Execute pip against the global bundled python.exe
    result = subprocess.run([r'{pythonExe}', '-m', 'pip', 'install', pkg], capture_output=True, text=True)
    
    import importlib
    importlib.invalidate_caches()
    
    if result.returncode == 0:
        return 'SUCCESS:\n' + result.stdout
    else:
        return 'ERROR:\n' + result.stderr
";
                        tempScope.Exec(code);
                        dynamic run_install_func = tempScope.Get("run_pip_install");

                        PyObject result = run_install_func(packageName);
                        return result.As<string>();
                    }
                }
                catch (Exception e)
                {
                    return $"CRITICAL ERROR: {e.Message}";
                }
            }
        });
    }

    public void Dispose()
    {
        // Check initialized status to avoid crashing on double-dispose
        if (PythonEngine.IsInitialized)
        {
            PythonEngine.Shutdown();
        }
    }
}