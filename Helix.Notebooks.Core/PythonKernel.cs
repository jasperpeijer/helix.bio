using System.Globalization;
using Python.Runtime;

namespace Helix.Notebooks.Core;

public class PythonKernel : IDisposable
{
    private PyModule _globalScope;

    public PythonKernel()
    {
        if (!PythonEngine.IsInitialized)
        {
            Runtime.PythonDLL = @"C:\Users\jaspe\miniconda3\python311.dll";
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }

        using (Py.GIL())
        {
            _globalScope = Py.CreateScope();
        }
    }

    public string Execute(string pythonCode)
    {
        using (Py.GIL())
        {
            try
            {
                string redirectCode = @"
import sys
import io
sys.stdout = io.StringIO()
sys.stderr = sys.stdout
";

                _globalScope.Exec(redirectCode);
                _globalScope.Exec(pythonCode);

                var output = _globalScope.Eval("sys.stdout.getvalue()").ToString(CultureInfo.InvariantCulture);

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

    public void Dispose()
    {
        _globalScope?.Dispose();

        if (PythonEngine.IsInitialized)
        {
            PythonEngine.Shutdown();
        }
    }
}