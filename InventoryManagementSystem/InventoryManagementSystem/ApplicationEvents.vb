Imports System.Runtime.InteropServices
Imports System.Threading

Namespace My
    Partial Friend Class MyApplication

        ' P/Invoke for immediate process termination
        <DllImport("kernel32.dll", SetLastError:=True)>
        Private Shared Function TerminateProcess(hProcess As IntPtr, uExitCode As UInteger) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)>
        Private Shared Function GetCurrentProcess() As IntPtr
        End Function

        ' Flag to track if we're in shutdown mode
        Private Shared isShuttingDown As Boolean = False

        ' === Handle Unhandled Exceptions ===
        Private Sub MyApplication_UnhandledException(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            ' ALWAYS suppress and force clean exit for ANY exception during app lifetime
            e.ExitApplication = False

            Debug.WriteLine($"Exception: {e.Exception.GetType().Name} - {e.Exception.Message}")

            ' Force immediate exit for AccessViolationException
            If TypeOf e.Exception Is AccessViolationException Then
                Debug.WriteLine("AccessViolationException - FORCE EXIT NOW")
                ForceImmediateTermination()
            Else
                ' For other exceptions, try graceful exit first
                Try
                    Environment.Exit(0)
                Catch
                    ForceImmediateTermination()
                End Try
            End If
        End Sub

        ' === Force Immediate Termination ===
        Private Sub ForceImmediateTermination()
            Try
                ' Method 1: TerminateProcess (most aggressive, bypasses ALL cleanup)
                TerminateProcess(GetCurrentProcess(), 0)
            Catch
                Try
                    ' Method 2: Environment.FailFast (no finally blocks, no cleanup)
                    Environment.FailFast("Force exit to prevent access violation")
                Catch
                    Try
                        ' Method 3: Process.Kill (kill own process)
                        Process.GetCurrentProcess().Kill()
                    Catch
                        ' If all else fails, just let it crash
                    End Try
                End Try
            End Try
        End Sub

        ' === Application Shutdown ===
        Private Sub MyApplication_Shutdown(sender As Object, e As EventArgs) Handles Me.Shutdown
            ' Don't do ANYTHING in shutdown
            ' Just let the process die immediately
            isShuttingDown = True
            ForceImmediateTermination()
        End Sub

        ' === Application Startup ===
        Private Sub MyApplication_Startup(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            Try
                ' Set up AppDomain exception handling
                AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppDomain_UnhandledException
            Catch ex As Exception
                Debug.WriteLine($"Startup error: {ex.Message}")
            End Try
        End Sub

        ' === AppDomain Level Exception Handler ===
        Private Sub AppDomain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
            Debug.WriteLine($"AppDomain Exception - forcing exit")
            ForceImmediateTermination()
        End Sub

    End Class
End Namespace