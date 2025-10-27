Public Class frmMainMenu
    Private isExiting As Boolean = False

    Private Sub btnManageInventory_Click(sender As Object, e As EventArgs) Handles btnManageInventory.Click
        If isExiting Then Return

        Dim inventoryForm As frmInventory = Nothing
        Try
            inventoryForm = New frmInventory()
            inventoryForm.ShowDialog()
        Catch ex As Exception
            If Not isExiting Then
                MessageBox.Show("Error opening Inventory form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If inventoryForm IsNot Nothing Then
                Try
                    inventoryForm.Dispose()
                Catch
                End Try
                inventoryForm = Nothing
            End If
        End Try
    End Sub

    Private Sub btnIssuedEquipment_Click(sender As Object, e As EventArgs) Handles btnIssuedEquipment.Click
        If isExiting Then Return

        Dim issuedForm As frmIssuedEquipment = Nothing
        Try
            issuedForm = New frmIssuedEquipment()
            issuedForm.ShowDialog()
        Catch ex As Exception
            If Not isExiting Then
                MessageBox.Show("Error opening Issued Equipment form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If issuedForm IsNot Nothing Then
                Try
                    issuedForm.Dispose()
                Catch
                End Try
                issuedForm = Nothing
            End If
        End Try
    End Sub

    Private Sub btnReports_Click(sender As Object, e As EventArgs) Handles btnReports.Click
        If isExiting Then Return

        Dim reportForm As frmReports = Nothing
        Try
            reportForm = New frmReports()
            reportForm.ShowDialog()
        Catch ex As Exception
            If Not isExiting Then
                MessageBox.Show("Error opening Reports form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If reportForm IsNot Nothing Then
                Try
                    reportForm.Dispose()
                Catch
                End Try
                reportForm = Nothing
            End If
        End Try
    End Sub

    Private Sub btnUserManagement_Click(sender As Object, e As EventArgs) Handles btnUserManagement.Click
        If isExiting Then Return

        If CurrentUser.IsAdmin() Then
            Dim userForm As frmUserManagement = Nothing
            Try
                userForm = New frmUserManagement()
                userForm.ShowDialog()
            Catch ex As Exception
                If Not isExiting Then
                    MessageBox.Show("Error opening User Management form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            Finally
                If userForm IsNot Nothing Then
                    Try
                        userForm.Dispose()
                    Catch
                    End Try
                    userForm = Nothing
                End If
            End Try
        Else
            MessageBox.Show("Access Denied! Only administrators can manage users.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop)
        End If
    End Sub

    Private Sub btnActivityLog_Click(sender As Object, e As EventArgs) Handles btnActivityLog.Click
        If isExiting Then Return

        If CurrentUser.IsAdmin() OrElse CurrentUser.IsManager() Then
            Dim activityForm As frmActivityLog = Nothing
            Try
                activityForm = New frmActivityLog()
                activityForm.ShowDialog()
            Catch ex As Exception
                If Not isExiting Then
                    MessageBox.Show("Error opening Activity Log form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            Finally
                If activityForm IsNot Nothing Then
                    Try
                        activityForm.Dispose()
                    Catch
                    End Try
                    activityForm = Nothing
                End If
            End Try
        Else
            MessageBox.Show("Access Denied! Only administrators and managers can view activity logs.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop)
        End If
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        If isExiting Then Return

        If MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                isExiting = True

                ' Stop the timer immediately
                Try
                    Timer1.Stop()
                    Timer1.Enabled = False
                Catch
                End Try

                ' Clear current user
                CurrentUser.Logout()

                ' Set dialog result and close this form
                Me.DialogResult = DialogResult.Retry ' Use Retry to signal logout
                Me.Close()

            Catch ex As Exception
                Debug.WriteLine("Logout error: " & ex.Message)
                ' Force exit on error
                Environment.Exit(0)
            End Try
        End If
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        If isExiting Then Return

        If MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            isExiting = True

            ' Stop timer immediately
            Try
                Timer1.Stop()
                Timer1.Enabled = False
            Catch
            End Try

            ' FORCE IMMEDIATE TERMINATION - bypass ALL cleanup
            ' This is the ONLY way to prevent access violations with COM/OLE DB
            Try
                ' Use Process.Kill to immediately terminate without cleanup
                Process.GetCurrentProcess().Kill()
            Catch
                Try
                    ' Fallback to TerminateProcess
                    Dim hProcess As IntPtr = Process.GetCurrentProcess().Handle
                    Dim succeeded As Boolean = TerminateProcess(hProcess, 0)
                    If Not succeeded Then
                        ' Last resort
                        Environment.Exit(0)
                    End If
                Catch
                    Environment.Exit(0)
                End Try
            End Try
        End If
    End Sub

    ' P/Invoke for TerminateProcess
    <Runtime.InteropServices.DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function TerminateProcess(hProcess As IntPtr, uExitCode As UInteger) As Boolean
    End Function

    Private Sub frmMainMenu_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.CenterToScreen()
        Me.Text = "Inventory Management System - Main Menu"

        ' Display logged-in user info
        lblWelcome.Text = "Welcome, " & CurrentUser.FullName & " (" & CurrentUser.UserRole & ")"

        ' Show/hide buttons based on role
        If CurrentUser.IsAdmin() Then
            btnUserManagement.Visible = True
            btnActivityLog.Visible = True
        ElseIf CurrentUser.IsManager() Then
            btnUserManagement.Visible = False
            btnActivityLog.Visible = True
        Else
            btnUserManagement.Visible = False
            btnActivityLog.Visible = False
        End If

        ' Set colors and styling
        Me.BackColor = Color.FromArgb(245, 245, 245)
        Panel1.BackColor = Color.FromArgb(0, 122, 204)
        lblWelcome.ForeColor = Color.White
        lblSystemTitle.ForeColor = Color.White
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If isExiting Then Return

        Try
            ' Update date/time display
            lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - hh:mm:ss tt")
        Catch
        End Try
    End Sub

    Private Sub frmMainMenu_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            isExiting = True

            ' Stop timer immediately
            Try
                Timer1.Stop()
                Timer1.Enabled = False
                RemoveHandler Timer1.Tick, AddressOf Timer1_Tick
            Catch
            End Try

            ' Remove all event handlers
            Try
                RemoveHandler btnManageInventory.Click, AddressOf btnManageInventory_Click
                RemoveHandler btnIssuedEquipment.Click, AddressOf btnIssuedEquipment_Click
                RemoveHandler btnReports.Click, AddressOf btnReports_Click
                RemoveHandler btnUserManagement.Click, AddressOf btnUserManagement_Click
                RemoveHandler btnActivityLog.Click, AddressOf btnActivityLog_Click
                RemoveHandler btnLogout.Click, AddressOf btnLogout_Click
                RemoveHandler btnExit.Click, AddressOf btnExit_Click
            Catch
            End Try

            ' DO NOTHING ELSE - let .NET handle cleanup

        Catch ex As Exception
            Debug.WriteLine("FormClosing error: " & ex.Message)
        End Try
    End Sub

    Private Sub frmMainMenu_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            GC.SuppressFinalize(Me)
        Catch
        End Try
    End Sub
End Class